using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Context;
using Shared.Infrastructure.Observability;
using Shared.Infrastructure.Redis.Config;
using Shared.Infrastructure.Redis.Interface.Core;
using Shared.Infrastructure.Redis.Model.Config;
using Shared.Kernel.Observability.Logging;
using StackExchange.Redis;
using System.Diagnostics;
using System.Reflection;
namespace Shared.Infrastructure.Redis.Core
{
    public class RedisConnectionManager : IRedisConnectionManger
    {

        private readonly ILogger _logger;
        private readonly ModuleMetaData _moduleMetaData;
        private Lazy<ConnectionMultiplexer> _lazyConnection { get; set; }
        private bool _disposed;
        private readonly Func<ConnectionMultiplexer> deferConnect;
        public RedisConnectionManager(
            ILogger logger, 
            IOptions<RedisOptions> options,
            IConfiguration configuration,
            ModuleMetaData moduleMeta )
        {
            _logger = logger.ForContext("Type", LoggerType.ModuleLog);
            var config = options.Value;
            _moduleMetaData = moduleMeta;
            deferConnect = () =>
            {
                var configuration = ConfigurationOptions.Parse(config.ConnectionString);
                configuration.AbortOnConnectFail = config.AbortOnConnectionFail;
                configuration.ConnectRetry = config.ConnectRetry;
                configuration.ConnectTimeout = 5000;
                configuration.KeepAlive = 180;
                // structure log
                var traceId = new TraceId(issuerId: moduleMeta.IssuerId, issuerType: moduleMeta.IssuerType);
                using (LogContext.PushProperty("TraceId", traceId))
                {
                    try
                    {
                        var con = ConnectionMultiplexer.Connect(configuration);
                        _logger.Information("Connecting to Redis: {Host}", config.ConnectionString);
                        return con;
                    }
                    catch (Exception ex)
                    {
                        _logger.Fatal(ex, "Failed to connect to Redis at {Host}", config.ConnectionString);
                        throw;
                    }
                }
            };
            _lazyConnection = new Lazy<ConnectionMultiplexer>(deferConnect);
        }

        public IDatabase GetDatabase(int db = -1) => _lazyConnection.Value.GetDatabase(db);

        public ISubscriber GetSubscriber() => _lazyConnection.Value.GetSubscriber();

        public IServer GetServer(string host, int port) => _lazyConnection.Value.GetServer(host, port);

        public IConnectionMultiplexer GetConnection() => _lazyConnection.Value;

        public async Task Dispose()
        {
            if (!_disposed)
            {
                if (_lazyConnection.IsValueCreated)
                {
                    await _lazyConnection.Value.DisposeAsync();
                }
                _disposed = true;
            }
            var traceId = new TraceId(issuerId: _moduleMetaData.IssuerId, issuerType: _moduleMetaData.IssuerType);
            using (LogContext.PushProperty("TraceId", traceId))
            {
                _logger.Verbose($"{Assembly.GetEntryAssembly()?.GetName().Name} Is Successfully disposed");
            }
        }

        public bool IsConnectionHealthy() => _lazyConnection.IsValueCreated && _lazyConnection.Value.IsConnected;

        public void AttemptHeal()
        {
            if (_lazyConnection.IsValueCreated && !_lazyConnection.Value.IsConnected)
            {
                _lazyConnection.Value.Dispose();
                _lazyConnection = new Lazy<ConnectionMultiplexer>(deferConnect);
            }
        }
    }
}
