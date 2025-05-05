using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Redis;
using System.Reflection;
namespace Shared.Kernel.Observability.HealthCheck
{
    public class RedisHealthCheck : IHealthCheck
    {
        private readonly StackExchange.Redis.IDatabase _redis;
        private readonly ILogger _logger;
        public const int DELAY_MS = 100;
        private const int MAX_ATTEMPTS = 3;

        public RedisHealthCheck(IRedisConnectionManger redis, ILogger<RedisHealthCheck> logger)
        {
            _redis = redis.GetDatabase(0);
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            string moduleName = Assembly.GetEntryAssembly().GetName().Name ?? "UnknownModule";
            for (int attempt = 1; attempt <= MAX_ATTEMPTS; attempt++)
            {
                try
                {
                    var pong = await _redis.ExecuteAsync("PING").ConfigureAwait(false);
                    if (pong.ToString() == "PONG")
                    {
                        return HealthCheckResult.Healthy($"{moduleName}:Redis is Healthy.");
                    }
                    _logger.LogWarning($"{moduleName}:Redis is Redis response not OK.");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"{moduleName}:Attempt {attempt}: Redis fail with {ex.Message}");
                }
                // Delay non-blocking
                await Task.Delay(DELAY_MS, cancellationToken).ConfigureAwait(false);
            }
            return HealthCheckResult.Unhealthy($"{moduleName}:Redis check failed after {MAX_ATTEMPTS} attempts.");
        }
    }
}
