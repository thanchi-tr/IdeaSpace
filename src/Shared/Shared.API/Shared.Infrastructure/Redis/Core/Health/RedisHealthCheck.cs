using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Reflection;
using Serilog;
using Shared.Infrastructure.Redis.Interface.Core;
using Microsoft.Extensions.Configuration;
using Shared.Kernel.Observability.Logging;
using Shared.Kernel.Interface.Health;
using Shared.Infrastructure.Observability;
using Serilog.Context;
using Shared.Kernel.Observability.Logging.Constant;
namespace Shared.Kernel.Observability.HealthCheck
{
    public class RedisHealthCheck : IEnrichHealthCheck
    {
        private readonly StackExchange.Redis.IDatabase _redis;
        private readonly Dictionary<LoggerType, ILogger> _logger;
        public const int DELAY_MS = 100;
        private const int MAX_ATTEMPTS = 3;

        public RedisHealthCheck(IRedisConnectionManger redis, ILogger logger, IConfiguration configuration)
        {
            _redis = redis.GetDatabase(-1);
            _logger = logger.InjectServiceType(ServiceType.Redis).Split();
        }

        /// <summary>
        /// This is a wrapper around basic health check that allow TraceId injection
        /// where the main module will have to inject the traceId including the 
        /// </summary>
        /// <param name="traceId"></param>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<HealthCheckResult> CheckHealthAsync(
            TraceId traceId, 
            HealthCheckContext context, 
            CancellationToken cancellationToken = default)
        {
            string moduleName = Assembly.GetEntryAssembly().GetName().Name ?? "UnknownModule";

            for (int attempt = 1; attempt <= MAX_ATTEMPTS; attempt++)
            {
                traceId.Refresh(); // ensure TraceId re-usability with up to date timestamp
                using (LogContext.PushProperty("TraceId", traceId))
                {
                    try
                    {
                        var pong = await _redis.ExecuteAsync("PING").ConfigureAwait(false);
                        if (pong.ToString() == "PONG")
                        {
                            _logger[LoggerType.ModuleLog].Verbose("{Module}:Redis is Healthy.", moduleName);
                            return HealthCheckResult.Healthy($"{moduleName}:Redis is Healthy.");
                        }
                        _logger[LoggerType.ModuleLog].Warning("{{Module}}:Redis is Redis response not OK.", moduleName);
                    }
                    catch (Exception ex)
                    {
                        _logger[LoggerType.ModuleLog].Warning("{Module}:Attempt {Attempt}: Redis fail with {Exception}", moduleName, attempt, ex.Message);
                    }
                    // Delay non-blocking
                    await Task.Delay(DELAY_MS, cancellationToken).ConfigureAwait(false);
                }
            }
            traceId.Refresh();
            using (LogContext.PushProperty("TraceId", traceId))
            {
                _logger[LoggerType.AuditLog].Fatal("{Module}:Redis check failed after Max  attempts.", moduleName);
                return HealthCheckResult.Unhealthy($"{moduleName}:Redis check failed after {MAX_ATTEMPTS} attempts.");
            }
        }

        
    }
}
