using IdeaSpace.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Shared.Domain.Constant.RabbitMQ;
using Shared.Messaging.Interface;
using Shared.Messaging.RabbitMq;

namespace ClientNotification.Infrastructure.Messaging.RabbitMQ.Config
{
    public class RabbitMqInitializer : BaseRabbitMqInitializer
    {
        public RabbitMqInitializer(ILogger<BaseRabbitMqInitializer> logger, IConfiguration configuration, IRabbitMqConnectionManager connectionManager) : base(logger, configuration, connectionManager)
        {
        }

        public override async Task<IChannel> Initialize()
        {
            var channel = await base.Initialize();

            // define the client mediator message exchange 
            
            await channel.ExchangeDeclareAsync(Exchange.CLIENT_MEDIATOR, ExchangeType.Topic, durable: true, autoDelete: true);
            await channel.InitQueue(Exchange.CLIENT_MEDIATOR, Queue.MEDIATOR_MODIFY_REQ);
            await channel.InitQueue(Exchange.CLIENT_MEDIATOR, Queue.MEDIATOR_GET_REQ);
            await channel.InitQueue(Exchange.CLIENT_MEDIATOR, Queue.MEDIATOR_CLEAN);
            return channel;
        }
    }
}
