using IdeaSpace.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
namespace Shared.Messaging
{
    public static class RabbitMqHostExtensions
    {
        public static async Task InitializeRabbitMqAsync(this IHost host)
        {
            using var scope = host.Services.CreateScope();

            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            var hostName = config["RabbitMQ:HostName"];
            var userName = config["RabbitMQ:UserName"];

            Console.WriteLine($"🐇 RabbitMQ Config: HostName = {hostName}, UserName = {userName}");

            var initializer = scope.ServiceProvider.GetRequiredService<RabbitMqInitializer>();
            await initializer.Initialize();
        }
    }
}
