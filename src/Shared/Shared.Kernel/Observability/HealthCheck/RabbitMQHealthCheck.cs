using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Exceptions;
using Shared.Domain.Constant.RabbitMQ;
using Shared.Messaging.Interface;
using System.Reflection;

namespace Shared.Kernel.Observability.HealthCheck
{
    public class RabbitMQHealthCheck : IHealthCheck
    {
        private readonly IRabbitMqConnectionManager _mq;
        private readonly ILogger<RabbitMQHealthCheck> _logger;
        public const int DELAY_MS = 100;
        private const int MAX_ATTEMPTS = 3;


        public RabbitMQHealthCheck(IRabbitMqConnectionManager mq, ILogger<RabbitMQHealthCheck> logger)
        {
            _mq = mq;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var moduleName = Assembly.GetEntryAssembly()?.GetName().Name ?? "UnknownModule";
            using var connection = await _mq.GetConnectionAsync();
            using var channel = await connection.CreateChannelAsync();
            for (int attempt = 1; attempt <= MAX_ATTEMPTS; attempt++)
            {

                try
                {
                    await channel.QueueDeclarePassiveAsync($"{Exchange.HEALTH}.queue").ConfigureAwait(false);
                    return HealthCheckResult.Healthy("RabbitMQ is Healthy.");
                }
                catch (OperationInterruptedException ex)
                {
                    _logger.LogWarning(ex, "{Module}: Attempt {Attempt} – RabbitMQ connection OK, but queue missing.", moduleName, attempt);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "{moduleName}:Attempt{attempt}:RabbitMQ is not reachable.", moduleName, attempt);
                }
                await Task.Delay(DELAY_MS, cancellationToken).ConfigureAwait(false);
            }

            return HealthCheckResult.Unhealthy($"{moduleName}: RabbitMQ failed after {MAX_ATTEMPTS} attempts.");
        }
    }
}
