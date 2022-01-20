using System.Threading.Tasks;
using ShareOut.Internal;
using StackExchange.Redis;

namespace ShareOut.Script
{
    class RedisScript : IRedisScript
    {
        private readonly RedisPersistConnectionManager _connection;

        public RedisScript(RedisPersistConnectionManager connection)
        {
            _connection = connection;
        }

        public async Task<LoadedLuaScript> LoadLuaScript(string scriptContents)
        {
            if (!_connection.TryGetServer(out var server))
            {
                throw new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Can not connect to Redis server");
            }
            var prepared = LuaScript.Prepare(scriptContents);
            return await prepared.LoadAsync(server);
        }

        public async Task<RedisResult> RunLuaScript(LoadedLuaScript script, RedisKey[] keys, RedisValue[] values)
        {
            if (!_connection.TryGetDatabase(out var database))
            {
                throw new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Can not connect to Redis server");
            }

            return await database.ScriptEvaluateAsync(script.Hash, keys, values);
        }
    }
}
