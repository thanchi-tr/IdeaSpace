
using Shared.Kernel.Observability.HealthCheck;
using Shared.Messaging.Interface.Contract;

namespace APIGateway.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            var envTag = builder.Configuration["Environment"] ?? "local";
            var config = builder.Configuration;

            builder.Services.Configure<RabbitMqOptions>(config.GetSection("RabbitMQ"));
            builder.Services.AddHostedService<Worker>();
            builder.Services.AddHealthChecks()
                .AddCheck<RabbitMQHealthCheck>(Infrastructure.Constant.Infrastructure.RabbitMQ, tags: new[] { "kernel", envTag });
            var host = builder.Build();
            host.Run();
        }
    }
}