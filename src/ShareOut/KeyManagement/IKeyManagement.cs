using System;
using System.Threading.Tasks;

namespace ShareOut.KeyManagement
{
    public interface IKeyManagement
    {
        Task<bool> KeyExistAsync(string key);
        bool KeyExist(string key);
        Task<bool> KeyExpireAsync(string key, TimeSpan expire);
        bool KeyExpire(string key, TimeSpan expire);
        Task RemoveAsync(params string[] keys);
    }
}
