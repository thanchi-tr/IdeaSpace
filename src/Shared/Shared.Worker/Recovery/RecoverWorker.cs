

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Shared.Worker.Recovery
{
    public class RecoverWorker : Microsoft.Extensions.Hosting.BackgroundService
    {
        protected readonly ILogger<RecoverWorker> _logger;
        private readonly HealthCheckService _healthCheckService;
        public const int CHECK_INTERVAL = 60 * 1000 * 10; // not sure if this is a good interval: 10minute
        public RecoverWorker(ILogger<RecoverWorker> logger, HealthCheckService healthCheckService)
        {
            _logger = logger;
            _healthCheckService = healthCheckService;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await _healthCheckService.CheckHealthAsync(cancellationToken);
                foreach (var entry in result.Entries)
                {
                    if (entry.Value.Status == HealthStatus.Healthy)
                    {

                    }
                    else
                    {
                        // pulisher an event to sys.recover.command.queue with the payload include the entry.Key (name of the service that fail)
                        // the Health check should log itsown break point
                        // Optional: fail fast or degrade here
                    }
                }

                await Task.Delay(CHECK_INTERVAL, cancellationToken);
            }
        }
    }
}