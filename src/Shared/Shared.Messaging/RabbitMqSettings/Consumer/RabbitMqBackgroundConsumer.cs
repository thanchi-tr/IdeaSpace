using IdeaSpace.Infrastructure.Interface.MessageBroker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Shared.Messaging.Consumer
{
    public abstract class RabbitMqBackgroundConsumer<TMessage> : BackgroundService, IAsyncDisposable
    {
        private readonly IRabbitMqConnectionFactory _factory;
        private readonly ILogger<RabbitMqBackgroundConsumer<TMessage>> _logger;
        private IChannel? _channel;
        private IConnection? _connection;

        protected RabbitMqBackgroundConsumer(
            IRabbitMqConnectionFactory factory,
            ILogger<RabbitMqBackgroundConsumer<TMessage>> logger)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected abstract string QueueName { get; }

        protected abstract Task HandleMessageAsync(TMessage message, CancellationToken cancellationToken);

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                _connection = await _factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                await _channel.QueueDeclareAsync(
                    queue: QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    cancellationToken: cancellationToken
                );

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += async (sender, ea) =>
                {
                    try
                    {
                        var message = DeserializeMessage(ea.Body.ToArray());
                        await HandleMessageAsync(message, cancellationToken);
                        await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message from queue '{QueueName}'.", QueueName);
                        await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken);
                    }
                };

                await _channel.BasicConsumeAsync(
                    queue: QueueName,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: cancellationToken
                );

                // Wait until cancellation is requested
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Background consumer for '{QueueName}' is stopping.", QueueName);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Critical failure in consumer for '{QueueName}'.", QueueName);
                throw;
            }
        }

        protected virtual TMessage DeserializeMessage(ReadOnlyMemory<byte> body)
        {
            return JsonSerializer.Deserialize<TMessage>(body.Span)
                   ?? throw new InvalidOperationException("Deserialization returned null.");
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel != null)
            {
                await _channel.CloseAsync();
                await _channel.DisposeAsync();
            }

            if (_connection != null)
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
            }

            GC.SuppressFinalize(this);
        }
    }
}
