using IdeaSpace.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Shared.Domain.Constant.RabbitMQ;
using Shared.Messaging.Interface;
using Shared.Messaging.RabbitMq;
namespace CatastrophicRecovery.Infrastructure.Messaging.RabbitMQ.Config
{
    public class RabbitMqInitializer : BaseRabbitMqInitializer
    {
        public RabbitMqInitializer(ILogger<BaseRabbitMqInitializer> logger, IConfiguration configuration, IRabbitMqConnectionManager connectionManager) : base(logger, configuration, connectionManager)
        {
        }

        /// <summary>
        /// Recovery service only interact with the recover exchange
        /// </summary>
        /// <returns></returns>
        public override async Task<IChannel> Initialize()
        {
            var channel = await base.Initialize();
            await channel.ExchangeDeclareAsync(Exchange.RECOVER, type: ExchangeType.Topic, durable: true);
            // only one exchange use for dead letter
            await channel.QueueDeclareAsync(Queue.HEALTH_CHECK, durable: true, exclusive: false, autoDelete: false);
            await channel.InitQueue(Exchange.RECOVER, Queue.DELTA_LOG, isWithDlq: true);
            await channel.InitQueue(Exchange.RECOVER, Queue.RECOVER_COMMAND, isWithDlq: true);
            await channel.InitQueue(Exchange.RECOVER, Queue.RECOVER_CLEAN, isWithDlq: true);

            return channel;
        }
    }
}
