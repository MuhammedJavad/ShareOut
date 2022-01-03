using ShareOut.Cursor;
using ShareOut.Lock;
using ShareOut.Messaging;
using StackExchange.Redis;

namespace ShareOut
{
    public interface IShare
    {
        Task<ICursor> Cursor(string key, string pattern);
        Task<IEnumerable<T>> RangeAsync<T>(string key, string score, CancellationToken token = default);
        Task<T> RankAsync<T>(string key, string score, CancellationToken token = default);
        ValueTask SortedSetAsync<T, TProperty>(
            string key, 
            Func<T, TProperty> scoreSelector, 
            IEnumerable<T> value,
            CancellationToken token = default);
        ValueTask SortedSetAsync<T>(string key, string score, T value, CancellationToken token = default);
        Task<T> GetAsync<T>(string key, CancellationToken token = default);
        Task<byte[]> GetAsync(string key, CancellationToken token = default);
        ValueTask SetAsync<T>(string key, T value, ShareEntryOptions options, CancellationToken token = default);
        ValueTask SetAsync(string key, byte[] value, ShareEntryOptions options, CancellationToken token = default);
        ValueTask RefreshAsync(string key, CancellationToken token = default);
        ValueTask IncrAsync(string key, CancellationToken token = default);
        ValueTask IncrByAsync(string key, int value, CancellationToken token = default);
        ValueTask RunScriptAsync(LoadedLuaScript script, CancellationToken token = default);
        ValueTask RemoveAsync(string key, CancellationToken token = default);
        Task<ILock> Lock(string key);
        ValueTask PublishAsync<T>(string key, T value, CancellationToken token = default);
        ValueTask SubscribeAsync(string key, IConsumer consumer);
        ValueTask<bool> PingAsync();
        bool Ping();
    }
}