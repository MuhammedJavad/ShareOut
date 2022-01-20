using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareOut.SortedSet
{
    public interface IRedisSortedSet
    {
        Task<IEnumerable<T>> RangeAsync<T>(string key, string score) where T : class, new();
        Task<T> RankAsync<T>(string key, string score) where T : class, new();
        Task<bool> SortedSetAsync<T>(string key, T value, double score) where T : class, new();
        Task<bool> SortedSetAsync(string key, byte[] value, double score);
    }
}
