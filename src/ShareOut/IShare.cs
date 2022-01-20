using System;
using System.Threading.Tasks;
using ShareOut.Cursor;
using ShareOut.HashSet;
using ShareOut.KeyManagement;
using ShareOut.Lock;
using ShareOut.Script;
using ShareOut.SortedSet;
using ShareOut.String;
using ShareOut.Subscription;

namespace ShareOut
{
    public interface IShare :
        IRedisString,
        IRedisHashSet,
        IRedisSortedSet,
        IRedisCursor,
        IKeyManagement,
        IRedisScript,
        IRedisLock,
        IRedisSubscription,
        IDisposable
    {
        Task<bool> PingAsync();
        bool Ping();
    }
}