using System.Threading.Tasks;

namespace ShareOut.Lock
{
    public interface IRedisLock
    {
        Task<ILockObject> Lock(string key);
    }
}
