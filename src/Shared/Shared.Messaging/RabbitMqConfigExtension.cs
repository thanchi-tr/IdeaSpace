using IdeaSpace.Infrastructure.Interface.MessageBroker;
using IdeaSpace.Infrastructure.Messaging.RabbitMq;
using IdeaSpace.Infrastructure;
using Microsoft.Extensions.DependencyInjection;


namespace Shared.Messaging
{
    public static class RabbitMqExtensions
    {
        public static IServiceCollection‎ ConfigureRabbitMq(this IServiceCollection‎ services)
        {
            services.AddSingleton<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();
            services.AddSingleton<RabbitMqInitializer>();
            return services;
        }
    }
}
