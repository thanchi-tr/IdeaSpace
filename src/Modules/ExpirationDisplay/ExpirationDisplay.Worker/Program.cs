using ExpirationDisplay.Infrastructure.Messaging.RabbitMQ.Config;
using IdeaSpace.Infrastructure;
using Shared.Messaging;
using Shared.Messaging.Interface.Contract;
namespace ExpirationDisplay.Worker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            var config = builder.Configuration;
            builder.Services.Configure<RabbitMqOptions>(config.GetSection("RabbitMQ"));
            builder.Services.AddHostedService<Worker>();
            //builder.Services.ConfigureRabbitMq();
            builder.Services.AddScoped<BaseRabbitMqInitializer, RabbitMqInitializer>();
            var host = builder.Build();
            await host.InitializeRabbitMqAsync();
            host.Run();
        }
    }
}