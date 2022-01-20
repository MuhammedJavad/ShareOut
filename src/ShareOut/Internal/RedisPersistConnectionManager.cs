using System;
using System.Linq;
using System.Timers;
using Microsoft.Extensions.Logging;
using Polly;
using StackExchange.Redis;

namespace ShareOut.Internal
{
    public class RedisPersistConnectionManager : IDisposable
    {
        private readonly ILogger<RedisPersistConnectionManager> _logger;
        private readonly ConfigurationOptions _options;
        private readonly object _connectionLocker = new();
        private readonly Timer _reconnectionTimer;
        private IConnectionMultiplexer _connection;

        public RedisPersistConnectionManager(
            RedisOptions options,
            ILogger<RedisPersistConnectionManager> logger)
        {
            KeyPrefix = $"{options.Instance}::";
            _options = Configure(options);
            _logger = logger;
            _reconnectionTimer = InitiateTimer();
            Connect();
        }

        public string KeyPrefix { get; }
        public bool IsConnected() => _connection is { IsConnected: true };
        public bool TryGetDatabase(out IDatabase database)
        {
            if (IsConnected())
            {
                database = _connection.GetDatabase();
                return true;
            }

            database = null;
            return false;
        }
        public bool TryGetServer(out IServer server)
        {
            if (IsConnected())
            {
                server = _connection.GetServer(_connection.GetEndPoints().First());
                return true;
            }

            server = null;
            return false;
        }
        public RedisKey[] GetKey(string[] keys) => keys.Select(GetKey).ToArray();
        public RedisKey GetKey(string key) => $"{KeyPrefix}{key}";
        public RedisChannel GetChannel(string key) => $"{KeyPrefix}Channel::{key}";

        private ConfigurationOptions Configure(RedisOptions redis)
        {
            var options = new ConfigurationOptions()
            {
                ClientName = redis.Instance,
                ConnectTimeout = redis.ConnectTimeout,
                SyncTimeout = redis.SyncTimeout,
                AsyncTimeout = redis.AsyncTimeout,
                ConnectRetry = redis.ConnectRetry,
                Password = redis.Password,
                Ssl = redis.Ssl
            };

            foreach (var endpoint in redis.Connections)
                options.EndPoints.Add(endpoint);

            return options;
        }

        private void Connect()
        {
            lock (_connectionLocker)
            {
                ClearConnection();

                var cnc = Policy.Handle<Exception>()
                    .WaitAndRetry(new[] { TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3) })
                    .ExecuteAndCapture(() => ConnectionMultiplexer.Connect(_options));

                if (cnc.Outcome == OutcomeType.Successful)
                {
                    _reconnectionTimer.Stop();
                    cnc.Result.ConnectionFailed += OnConnectionLost;
                    cnc.Result.InternalError += InternalError;
                    _connection = cnc.Result;
                }
                
                _reconnectionTimer.Start();
            }
        }

        private void OnConnectionLost(object sender, ConnectionFailedEventArgs args)
        {
            _logger.LogError(
                args.Exception,
                "Redis connection multiplexer has lost the connection",
                args.ConnectionType.ToString(),
                args.EndPoint);
            Connect();
        } 

        private void InternalError(object sender, InternalErrorEventArgs args)
        {
            _logger.LogError(
                args.Exception, 
                "An internal error occurred in Redis connection multiplexer", 
                args.Origin, 
                args.ConnectionType.ToString(), 
                args.EndPoint);

            Reconnect();
        }

        private Timer InitiateTimer()
        {
            var timer = new Timer()
            {
                Enabled = true,
                AutoReset = true,
                Interval = TimeSpan.FromMinutes(1).TotalMilliseconds,
            };

            timer.Elapsed += TimerOnElapsed;

            return timer;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e) => Reconnect();

        private void Reconnect()
        {
            if (!IsConnected()) Connect();
        }

        private bool _disposed;
        public void Dispose()
        {
            if (_disposed) return;
            ClearConnection();
            DisposeTimer();
            _disposed = true;

            void DisposeTimer()
            {
                if (_reconnectionTimer == null) return;
                _reconnectionTimer.Elapsed -= TimerOnElapsed;
                _reconnectionTimer.Dispose();
            }
        }

        private void ClearConnection()
        {
            if (_connection == null) return;
            _connection.ConnectionFailed -= OnConnectionLost;
            _connection.InternalError -= InternalError;
            _connection.Dispose();
            _connection = null;
        }
    }
}
