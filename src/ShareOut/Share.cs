using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ShareOut.Cursor;
using ShareOut.HashSet;
using ShareOut.Internal;
using ShareOut.Lock;
using ShareOut.Script;
using ShareOut.Subscription;
using StackExchange.Redis;

namespace ShareOut
{
    public class Share : IShare
    {
        #region ctor

        private readonly RedisPersistConnectionManager _connection;
        private readonly IRedisScript _script;
        private readonly IRedisHashSet _hashSet;

        public Share(RedisPersistConnectionManager connection)
        {
            _connection = connection;
            _script = new RedisScript(_connection);
            _hashSet = new RedisHashSet(_connection, _script);
        }

        #endregion

        #region SortedSet

        public Task<IEnumerable<T>> RangeAsync<T>(string key, string score)
            where T : class, new()
        {
            throw new NotImplementedException();
        }

        public Task<T> RankAsync<T>(string key, string score)
            where T : class, new()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SortedSetAsync<T>(string key, T value, double score)
            where T : class, new()
        {
            return await SortedSetAsync(key, value.CompressValue(), score);
        }

        public async Task<bool> SortedSetAsync(string key, byte[] value, double score)
        {
            if (!_connection.TryGetDatabase(out var database)) return false;
            return await database.SortedSetAddAsync(_connection.GetKey(key), value, score);
        }

        #endregion

        #region RedisHashSet

        public Task<T> AccumulateHashAsync<T>(string key, string field, Task<T> loadTask, TimeSpan? expire)
            where T : class, new()
        {
            return _hashSet.AccumulateHashAsync(key, field, loadTask, expire);
        }

        public Task<T> HashGetAsync<T>(string key, string field)
            where T : class, new()
        {
            return _hashSet.HashGetAsync<T>(key, field);
        }

        public Task<byte[]> HashGetAsync(string key, string field)
        {
            return _hashSet.HashGetAsync(key, field);
        }

        public Task<long> HashGetNumberAsync(string key, string field)
        {
            return _hashSet.HashGetNumberAsync(key, field);
        }

        public Task HashSetAsync<T>(string key, string field, T value, TimeSpan? expire)
            where T : class, new()
        {
            return _hashSet.HashSetAsync(key, field, value, expire);
        }

        public Task<long> HashIncrAsync(string key, string field)
        {
            return _hashSet.HashIncrAsync(key, field);
        }

        public Task HashSetAsync(string key, string field, byte[] value, TimeSpan? expire)
        {
            return _hashSet.HashSetAsync(key, field, value, expire);
        }

        public Task HashSetAsync<T>(
            string key,
            IEnumerable<T> value,
            Func<T, string> fieldSelector,
            TimeSpan? expireTime)
            where T : class, new()
        {
            return _hashSet.HashSetAsync(key, value, fieldSelector, expireTime);
        }

        #endregion

        #region String

        public async Task<List<KeyValuePair<string, object>>> GetManyAsync(params string[] keys)
        {
            if (keys is not { Length: > 0 })
            {
                throw new Exception();
            }

            if (!_connection.TryGetDatabase(out var database))
            {
                return new List<KeyValuePair<string, object>>();
            }

            var result = await database.StringGetAsync(_connection.GetKey(keys));

            if (result is not { Length: > 0 })
            {
                return new List<KeyValuePair<string, object>>();
            }

            return result.Select((val, index) =>
                    new KeyValuePair<string, object>(
                        keys[index],
                        JsonSerializer.Deserialize<object>(val)))
                .ToList();
        }

        public async Task<T> AccumulateStringAsync<T>(string key, Task<T> loadTask, TimeSpan? expire)
            where T : class, new()
        {
            var cachedObj = await StringGetAsync<T>(key);

            if (cachedObj != null) return cachedObj;

            var obj = await loadTask;

            if (obj == null) return default;

            await StringSetAsync(key, obj, expire);

            return obj;
        }

        public async Task<T> StringGetAsync<T>(string key)
            where T : class, new()
        {
            var result = await StringGetAsync(key);
            return result.Deserialize<T>();
        }

        public async Task<byte[]> StringGetAsync(string key)
        {
            if (!_connection.TryGetDatabase(out var database)) return Array.Empty<byte>();
            var result = await database.StringGetAsync(_connection.GetKey(key));
            return result.DecompressValue();
        }

        public Task<bool> StringSetAsync<T>(string key, T value, TimeSpan? expire)
            where T : class, new()
        {
            return StringSetAsync(key, value.CompressValue(), expire);
        }

        public async Task<bool> StringSetAsync(string key, byte[] value, TimeSpan? expire)
        {
            if (!_connection.TryGetDatabase(out var database)) return false;
            return await database.StringSetAsync(_connection.GetKey(key), value, expire);
        }

        public async Task<long> IncrAsync(string key)
        {
            if (!_connection.TryGetDatabase(out var database)) return 0;
            return await database.StringIncrementAsync(_connection.GetKey(key));
        }

        public async Task<long> IncrByAsync(string key, long value)
        {
            if (value < 1)
                throw new ArgumentException("Argument must be greater than zero", nameof(value));

            if (!_connection.TryGetDatabase(out var database)) return 0;

            return await database.StringIncrementAsync(_connection.GetKey(key), value);
        }

        #endregion

        #region Pub/Sub

        public async Task PublishAsync<T>(string key, T value)
            where T : class, new()
        {
            if (!_connection.TryGetDatabase(out var database)) return;

            await database.PublishAsync(
                    _connection.GetChannel(key),
                    value.CompressValue(),
                    CommandFlags.FireAndForget);
        }

        public Task SubscribeAsync(string key, IConsumer consumer)
        {
            throw new NotImplementedException();
        }


        #endregion

        #region Health

        public async Task<bool> PingAsync()
        {
            if (!_connection.TryGetDatabase(out var database)) return false;
            var latency = await database.PingAsync();
            return latency < TimeSpan.FromSeconds(1);
        }

        public bool Ping()
        {
            if (!_connection.TryGetDatabase(out var database)) return false;
            var latency = database.Ping();
            return latency < TimeSpan.FromSeconds(1);
        }

        #endregion

        #region Script

        public Task<LoadedLuaScript> LoadLuaScript(string scriptContents)
        {
            return _script.LoadLuaScript(scriptContents);
        }

        public Task<RedisResult> RunLuaScript(LoadedLuaScript script, RedisKey[] keys, RedisValue[] values)
        {
            return _script.RunLuaScript(script, keys, values);
        }

        #endregion

        #region Key Managment

        public async Task<bool> KeyExistAsync(string key)
        {
            if (!_connection.TryGetDatabase(out var database)) return false;
            return await database.KeyExistsAsync(_connection.GetKey(key));
        }

        public bool KeyExist(string key)
        {
            if (!_connection.TryGetDatabase(out var database)) return false;
            return database.KeyExists(_connection.GetKey(key));
        }

        public async Task<bool> KeyExpireAsync(string key, TimeSpan expire)
        {
            if (!_connection.TryGetDatabase(out var database)) return false;
            return await database.KeyExpireAsync(_connection.GetKey(key), expire);
        }

        public bool KeyExpire(string key, TimeSpan expire)
        {
            return _connection.TryGetDatabase(out var database) &&
                   database.KeyExpire(_connection.GetKey(key), expire);
        }

        public async Task RemoveAsync(params string[] keys)
        {
            if (!_connection.TryGetDatabase(out var database)) return;
            await database.KeyDeleteAsync(_connection.GetKey(keys));
        }

        #endregion

        public Task<IRedisCursor> Cursor(string key, string pattern)
        {
            throw new NotImplementedException();
        }

        public Task<ILockObject> Lock(string key)
        {
            throw new NotImplementedException();
        }

        private bool _disposed;
        public void Dispose()
        {
            if (_disposed) return;
            _connection.Dispose();
            _disposed = true;
        }
    }
}
