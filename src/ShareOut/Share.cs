using ShareOut.Cursor;
using ShareOut.Lock;
using ShareOut.Messaging;
using StackExchange.Redis;

namespace ShareOut
{
    public class Share : IShare
    {
        public Task<ICursor> Cursor(string key, string pattern)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> RangeAsync<T>(string key, string score, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<T> RankAsync<T>(string key, string score, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask SortedSetAsync<T, TProperty>(
            string key, 
            Func<T, TProperty> scoreSelector, 
            IEnumerable<T> value,
            CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask SortedSetAsync<T>(string key, string score, T value, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetAsync<T>(string key, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask SetAsync<T>(string key, T value, ShareEntryOptions options, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask SetAsync(string key, byte[] value, ShareEntryOptions options, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask RefreshAsync(string key, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask IncrAsync(string key, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask IncrByAsync(string key, int value, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask RunScriptAsync(LoadedLuaScript script, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask RemoveAsync(string key, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<ILock> Lock(string key)
        {
            throw new NotImplementedException();
        }

        public ValueTask PublishAsync<T>(string key, T value, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask SubscribeAsync(string key, IConsumer consumer)
        {
            throw new NotImplementedException();
        }

        public ValueTask<bool> PingAsync()
        {
            throw new NotImplementedException();
        }

        public bool Ping()
        {
            throw new NotImplementedException();
        }
    }
}