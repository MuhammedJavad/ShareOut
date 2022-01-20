using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareOut.HashSet
{
    public interface IRedisHashSet
    {
        Task<T> AccumulateHashAsync<T>(string key, string field, Task<T> loadTask, TimeSpan? expire) where T : class, new();
        Task<T> HashGetAsync<T>(string key, string field) where T : class, new();
        Task<byte[]> HashGetAsync(string key, string field);
        Task<long> HashGetNumberAsync(string key, string field);
        Task<long> HashIncrAsync(string key, string field);
        Task HashSetAsync<T>(string key, string field, T value, TimeSpan? expire) where T : class, new();
        Task HashSetAsync(string key, string field, byte[] value, TimeSpan? expire);
        Task HashSetAsync<T>(
            string key,
            IEnumerable<T> value,
            Func<T, string> fieldSelector,
            TimeSpan? expireTime)
            where T : class, new();
    }
}
