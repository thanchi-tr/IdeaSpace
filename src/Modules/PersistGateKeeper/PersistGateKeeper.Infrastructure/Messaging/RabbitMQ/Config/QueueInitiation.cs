using IdeaSpace.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Shared.Domain.Constant.RabbitMQ;
using Shared.Messaging.Interface;
using Shared.Messaging.RabbitMq;
namespace PersistGateKeeper.Infrastructure.Messaging.RabbitMQ.Config
{
    public class RabbitMqInitializer : BaseRabbitMqInitializer
    {
        public RabbitMqInitializer(ILogger<BaseRabbitMqInitializer> logger, IConfiguration configuration, IRabbitMqConnectionManager connectionManager) : base(logger, configuration, connectionManager)
        {
        }

        /// <summary>
        /// GateKeeper service interact with 
        /// (1) - included Recover exchange
        /// (2) - asynchonous operation (domain)
        /// </summary>
        /// <returns></returns>
        public override async Task<IChannel> Initialize()
        {
            var channel = await base.Initialize();

            // define the async ops exchange and its queue
            await channel.ExchangeDeclareAsync(Exchange.ASYNC_OPERATE, type: ExchangeType.Topic, durable: true);
            await channel.InitQueue(Exchange.ASYNC_OPERATE, Queue.CACHING_OPERATION, isWithDlq:true);
            await channel.InitQueue(Exchange.ASYNC_OPERATE, Queue.MODIFY_OPERATION, isWithDlq: true);
            await channel.InitQueue(Exchange.ASYNC_OPERATE, Queue.CREATE_OPERATION, isWithDlq: true);
            return channel;
        }
    }
}
