using System.Threading.Tasks;

namespace ShareOut.Lock
{
    public interface ILockObject
    {
        ValueTask Release();
    }
}
