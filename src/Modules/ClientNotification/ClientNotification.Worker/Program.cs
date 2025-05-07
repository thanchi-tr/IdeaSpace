using Shared.Messaging.Interface.Contract;

namespace ClientNotification.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            var config = builder.Configuration;
            builder.Services.Configure<RabbitMqOptions>(config.GetSection("RabbitMQ"));
            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
    }
}