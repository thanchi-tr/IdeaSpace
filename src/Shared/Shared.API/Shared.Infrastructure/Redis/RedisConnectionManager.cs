using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
namespace Shared.Infrastructure.Redis
{
    public class RedisConnectionManager : IRedisConnectionManger
    {

        private readonly ILogger<RedisConnectionManager> _logger;
        private readonly Lazy<ConnectionMultiplexer> _lazyConnection;
        private bool _disposed;

        public RedisConnectionManager(
            ILogger<RedisConnectionManager> logger, 
            IOptions<RedisOptions> options)
        {
            _logger = logger;
            var config = options.Value;
            _lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                var configuration = ConfigurationOptions.Parse(config.ConnectionString);
                configuration.AbortOnConnectFail = config.AbortOnConnectionFail;
                configuration.ConnectRetry = config.ConnectRetry;
                configuration.ConnectTimeout = 5000;
                configuration.KeepAlive = 180;

                _logger.LogInformation("Connecting to Redis: {Host}", config.ConnectionString);
                return ConnectionMultiplexer.Connect(configuration);
            });
        }

        public StackExchange.Redis.IDatabase GetDatabase(int db = -1) => _lazyConnection.Value.GetDatabase(db);

        public ISubscriber GetSubscriber() => _lazyConnection.Value.GetSubscriber();

        public IServer GetServer(string host, int port) => _lazyConnection.Value.GetServer(host, port);

        public IConnectionMultiplexer GetConnection() => _lazyConnection.Value;

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_lazyConnection.IsValueCreated)
                {
                    _lazyConnection.Value.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
