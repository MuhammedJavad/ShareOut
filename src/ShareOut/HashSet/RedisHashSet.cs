using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShareOut.Internal;
using ShareOut.Script;
using StackExchange.Redis;

namespace ShareOut.HashSet
{
    class RedisHashSet : IRedisHashSet
    {
        private readonly RedisPersistConnectionManager _connection;
        private readonly IRedisScript _redisScript;

        public RedisHashSet(RedisPersistConnectionManager connection, IRedisScript redisScript)
        {
            _connection = connection;
            _redisScript = redisScript;
        }

        public async Task<T> AccumulateHashAsync<T>(
            string key,
            string field,
            Task<T> loadTask,
            TimeSpan? expire) where T : class, new()
        {
            var cachedObj = await HashGetAsync<T>(key, field);

            if (cachedObj != null) return cachedObj;

            var obj = await loadTask;

            if (obj == null) return default;

            await HashSetAsync(key, field, obj, expire);

            return obj;
        }

        public async Task<T> HashGetAsync<T>(string key, string field) where T : class, new()
        {
            var result = await HashGetAsync(key, field);
            return result.Deserialize<T>();
        }

        public async Task<byte[]> HashGetAsync(string key, string field)
        {
            if (!_connection.TryGetDatabase(out var database)) return Array.Empty<byte>();
            var result = await database.HashGetAsync(_connection.GetKey(key), field);
            return result.DecompressValue();
        }

        public async Task<long> HashGetNumberAsync(string key, string field)
        {
            if (!_connection.TryGetDatabase(out var database)) return 0;
            var result = await database.HashGetAsync(_connection.GetKey(key), field);
            return result.TryParse(out long value) ? value : 0;
        }

        public async Task<long> HashIncrAsync(string key, string field)
        {
            if (!_connection.TryGetDatabase(out var database)) return 0;
            return await database.HashIncrementAsync(_connection.GetKey(key), field);
        }

        public async Task HashSetAsync<T>(string key, string field, T value, TimeSpan? expire) where T : class, new()
        {
            await HashSetAsync(key, field, value.CompressValue(), expire);
        }

        public async Task HashSetAsync(string key, string field, byte[] value, TimeSpan? expire)
        {
            if (!_connection.IsConnected()) return;

            await RunSetScript(
                key,
                new RedisKey[] { field },
                new RedisValue[] { value },
                expire);
        }

        public async Task HashSetAsync<T>(
            string key,
            IEnumerable<T> value,
            Func<T, string> fieldSelector,
            TimeSpan? expireTime) where T : class, new()
        {
            if (!_connection.IsConnected()) return;

            var fields = new List<RedisKey>();
            var values = new List<RedisValue>();

            foreach (var val in value)
            {
                fields.Add(fieldSelector.Invoke(val));
                values.Add(val.CompressValue());
            }

            await RunSetScript(key, fields.ToArray(), values.ToArray(), expireTime);
        }

        private async Task RunSetScript(string key, RedisKey[] fields, RedisValue[] values, TimeSpan? expire)
        {
            var script = await CreateSetScript(fields.Length);
            await _redisScript.RunLuaScript(
                script,
                fields.Prepend(_connection.GetKey(key)).ToArray(),
                values.Prepend(expire?.TotalSeconds ?? -1L).ToArray());
        }

        private async Task<LoadedLuaScript> CreateSetScript(int fieldsCount)
        {
            var scriptString = new StringBuilder();

            scriptString.Append("redis.call('HMSET',KEYS[1]");

            for (var i = 0; i < fieldsCount; i++)
            {
                scriptString.Append($",KEYS[{i + 2}],ARGV[{i + 2}]");
            }

            scriptString.Append(")\r\nif ARGV[1] ~= '-1' then\r\nredis.call('EXPIRE',KEYS[1],ARGV[1])\r\nend\r\nreturn 1");

            return await _redisScript.LoadLuaScript(scriptString.ToString());
        }
    }
}
