using RabbitMQ.Client;
namespace CatastrophicRecovery.Infrastructure.Messaging.RabbitMQ.Config
{
    public static class QueueInitionation
    {
        public static async Task<IChannel> InitQueue(this IChannel channel)
        {

            return channel;
        }
    }
}
