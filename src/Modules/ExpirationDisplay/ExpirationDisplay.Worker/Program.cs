using Shared.Messaging;
namespace ExpirationDisplay.Worker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<Worker>();
            builder.Services.ConfigureRabbitMq();
            var host = builder.Build();
            await host.InitializeRabbitMqAsync();
            host.Run();
        }
    }
}