using IdeaSpace.Infrastructure;
using RabbitMQ.Client;
using Shared.Domain.Constant.RabbitMQ;
using Shared.Messaging.Interface;
using Shared.Messaging.RabbitMq;

namespace ExpirationDisplay.Infrastructure.Messaging.RabbitMQ.Config
{
    public class RabbitMqInitializer : BaseRabbitMqInitializer
    {
        public RabbitMqInitializer(Microsoft.Extensions.Logging.ILogger<BaseRabbitMqInitializer> logger, Microsoft.Extensions.Configuration.IConfiguration configuration, IRabbitMqConnectionManager connectionManager) : base(logger, configuration, connectionManager)
        {
        }

        public override async Task<IChannel> Initialize()
        {
            var channel = await base.Initialize();
            await channel.ExchangeDeclareAsync(Exchange.SYNC_OPERATE,type: ExchangeType.Topic, durable: true, autoDelete: false);
            await channel.InitQueue(Exchange.SYNC_OPERATE, Queue.VIEW_OPERATION);
            return channel;
        }
    } 
}
