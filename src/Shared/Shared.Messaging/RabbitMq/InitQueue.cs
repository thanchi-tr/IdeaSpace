using RabbitMQ.Client;
using Shared.Domain.Constant.RabbitMQ;

namespace Shared.Messaging.RabbitMq
{
    public static  class QueueInitionation
    {
        /// <summary>
        /// At this moment, we wont dive much deeper into 
        /// 1. retry machanism
        /// 2. TTL + evict policy just yet
        /// @todo: modify function for it later
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="exchangeName"></param>
        /// <param name="queueName"></param>
        /// <param name="isWithDlq"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<IChannel> InitQueue(
            this IChannel channel,
            string exchangeName,
            string queueName,
            bool isWithDlq = true)
        {
            // we dont need to return an empty container,
            // this method will be call  during the initiation stage,
            // we prefer fail right way if unhandle.
            if (string.IsNullOrWhiteSpace(exchangeName))
                throw new ArgumentException("Exchange name cannot be null or empty.", nameof(exchangeName));
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Queue name cannot be null or empty.", nameof(queueName));

            var args = new Dictionary<string, object>();
            
            if (isWithDlq)
            {
                args.Add("x-dead-letter-exchange", Exchange.DLX);
                await channel.QueueDeclareAsync($"{queueName}.dead.letter.queue", durable: true, exclusive: false, autoDelete: false);
                await channel.QueueBindAsync($"{queueName}.dead.letter.queue", exchange: Exchange.DLX, routingKey: queueName);

            }
            await channel.QueueDeclareAsync($"{queueName}.queue", durable: true, exclusive: false, autoDelete: false, arguments: args);
            await channel.QueueBindAsync($"{queueName}.queue", exchange: exchangeName, routingKey: queueName);
            return channel;
        }
    }
}
