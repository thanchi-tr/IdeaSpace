using IdeaSpace.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Messaging.Interface.Contract;

namespace Shared.Messaging
{
    public static class RabbitMqHostExtensions
    {
        public static async Task InitializeRabbitMqAsync(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var initializer = scope.ServiceProvider.GetRequiredService<BaseRabbitMqInitializer>();
            await initializer.Initialize();
        }
    }
}
