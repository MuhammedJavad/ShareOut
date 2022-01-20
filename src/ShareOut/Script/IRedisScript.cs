using System.Threading.Tasks;
using StackExchange.Redis;

namespace ShareOut.Script
{
    public interface IRedisScript
    {
        Task<LoadedLuaScript> LoadLuaScript(string scriptContents);
        Task<RedisResult> RunLuaScript(LoadedLuaScript script, RedisKey[] keys, RedisValue[] values);
    }
}
