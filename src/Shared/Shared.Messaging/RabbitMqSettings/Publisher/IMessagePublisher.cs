using IdeaSpace.Infrastructure.Interface.MessageBroker;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Shared.Messaging.Interface.Publisher
{
    public class RabbitMqMessagePublisher<T> : IMessagePublisher<T>
    {
        private readonly IRabbitMqConnectionFactory _factory;

        public RabbitMqMessagePublisher(IRabbitMqConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task PublishAsync(T message)
        {
            using var connection = await _factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

       
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));


            await channel.BasicPublishAsync(
                exchange: "idea.exchange",
                routingKey: "",
                body: body);


        }
    }

}
