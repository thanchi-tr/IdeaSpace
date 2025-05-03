using IdeaSpace.Infrastructure;
using PersistGateKeeper.Infrastructure.Messaging.RabbitMQ.Config;
using Shared.Messaging;

namespace CatastrophicRecovery.Worker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<Worker>();
            builder.Services.AddScoped<BaseRabbitMqInitializer,RabbitMqInitializer>();
            var host = builder.Build();
            await host.InitializeRabbitMqAsync();
            host.Run();
        }
    }
}