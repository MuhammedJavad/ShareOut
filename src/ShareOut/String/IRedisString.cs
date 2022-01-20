using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareOut.String
{
    public interface IRedisString
    {
        Task<List<KeyValuePair<string, object>>> GetManyAsync(params string[] keys);
        Task<T> AccumulateStringAsync<T>(string key, Task<T> loadTask, TimeSpan? expire) where T : class, new();
        Task<T> StringGetAsync<T>(string key) where T : class, new();
        Task<byte[]> StringGetAsync(string key);
        Task<bool> StringSetAsync<T>(string key, T value, TimeSpan? expire) where T : class, new();
        Task<bool> StringSetAsync(string key, byte[] value, TimeSpan? expire);
        Task<long> IncrAsync(string key);
        Task<long> IncrByAsync(string key, long value);
    }
}
