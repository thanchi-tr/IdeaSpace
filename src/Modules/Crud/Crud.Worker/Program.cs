using Shared.Messaging;
namespace Crud.Worker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<Worker>()
                    .ConfigureRabbitMq();

            var host = builder.Build();
            await host.InitializeRabbitMqAsync();
            host.Run();
        }
    }
}