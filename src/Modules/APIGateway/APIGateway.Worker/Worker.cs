using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;
using Shared.Worker.Messaging;
using Shared.Worker.Recovery;

namespace APIGateway.Worker
{
    /// <summary>
    /// API Gateway worker access rabbitMQ (so it consume
    /// </summary>
    public class Worker : DeadLetterWorkerBase<string>
    {
        public Worker(IChannel channel, string queueName, Serilog.ILogger logger, HealthCheckService healthCheckService) : base(channel, queueName, logger)
        {
        }

        protected override Task HandleDeadLetterAsync(string payload, int retryCount, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
