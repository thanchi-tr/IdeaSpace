using IdeaSpace.Infrastructure;
using PersistGateKeeper.Infrastructure.Messaging.RabbitMQ.Config;
using Shared.Messaging;
using Shared.Messaging.Interface.Contract;

namespace CatastrophicRecovery.Worker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<Worker>();
            var config = builder.Configuration;
            builder.Services.Configure<RabbitMqOptions>(config.GetSection("RabbitMQ"));
            builder.Services.AddScoped<BaseRabbitMqInitializer,RabbitMqInitializer>();
            var host = builder.Build();
            await host.InitializeRabbitMqAsync();
            host.Run();
        }
    }
}