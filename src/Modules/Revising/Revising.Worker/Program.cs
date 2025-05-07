using IdeaSpace.Infrastructure;
using Shared.Messaging;
using Shared.Messaging.Interface.Contract;
namespace Revising.Worker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            var config = builder.Configuration;
            builder.Services.Configure<RabbitMqOptions>(config.GetSection("RabbitMQ"));
            builder.Services.AddHostedService<Worker>();
            builder.Services.AddScoped<BaseRabbitMqInitializer,BaseRabbitMqInitializer>();
            var host = builder.Build();
            await host.InitializeRabbitMqAsync();
            host.Run();
        }
    }
}