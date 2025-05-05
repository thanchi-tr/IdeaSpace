
using Shared.Kernel.Observability.HealthCheck;

namespace APIGateway.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            var envTag = builder.Configuration["Environment"] ?? "local";
            builder.Services.AddHostedService<Worker>();

            builder.Services.AddHealthChecks()
                .AddCheck<RabbitMQHealthCheck>(Infrastructure.Constant.Infrastructure.RabbitMQ, tags: new[] { "kernel", envTag });
            var host = builder.Build();
            host.Run();
        }
    }
}