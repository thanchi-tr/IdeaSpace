using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shared.Infrastructure.Observability;

namespace Shared.Kernel.Interface.Health
{
    public interface IEnrichHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(TraceId traceid, HealthCheckContext context, CancellationToken cancellationToken = default);
    }
}
