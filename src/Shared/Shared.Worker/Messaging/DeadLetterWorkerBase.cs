using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Shared.Worker.Messaging
{
    /// <summary>
    /// This by convention, attach and consume event from one of the dlq,
    /// the routing/retry + exist handling logic is define by concrete class (via HandleDeadLetterAsync)
    /// </summary>
    /// <typeparam name="TPayload">
    ///     
    /// </typeparam>
    public abstract class DeadLetterWorkerBase<TPayload> : BackgroundService
    {
        private readonly IChannel _channel;
        private readonly string _queueName;
        private readonly ILogger<DeadLetterWorkerBase<TPayload>> _logger;

        private CancellationTokenSource? _ctoken;
        private Task? _bgTask;
        protected DeadLetterWorkerBase(IChannel channel, string queueName, ILogger<DeadLetterWorkerBase<TPayload>> logger)
        {
            _channel = channel;
            _queueName = queueName;
            _logger = logger;

        }
        public async Task AttachDLQConsumerAsync(CancellationToken token)
        {
            
            var moduleName = Assembly.GetEntryAssembly().GetName().Name;
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var payload = JsonSerializer.Deserialize<TPayload>(json);

                    var xDeathCount = GetRetryCount(ea.BasicProperties.Headers);

                    _logger.LogInformation($"{moduleName}:DLQ for {_queueName}:Message received with retry count {xDeathCount}");

                    await HandleDeadLetterAsync(payload, xDeathCount, token);

                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, token);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, $"{moduleName}.DLQWorker:DLQ for {_queueName}:Fail to deserialise Payload");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{moduleName}.DLQWorker:DLQ for {_queueName}: Failed to handle message");
                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, token); // discard to avoid poison loop
                }
            };

            await _channel.BasicConsumeAsync(_queueName,autoAck:false, consumer, token);
        }

        private int GetRetryCount(IDictionary<string, object> headers)
        {
            if (headers == null || !headers.TryGetValue("x-death", out var xDeathRaw))
                return 0;

            try
            {
                var xDeathList = xDeathRaw as IList<object>;
                var xDeathEntry = xDeathList?[0] as IDictionary<string, object>;
                if (xDeathEntry != null && xDeathEntry.TryGetValue("count", out var count))
                {
                    return Convert.ToInt32(count);
                }
            }
            catch
            {
                // ignore parsing error
            }
            return 0;
        }

        /// <summary>
        /// Override this method to implement custom handling logic
        /// This also specify the retry machanism (update the count and send to DLQ or dispose and call the respective handler subject)
        /// such as cleanup, audit logging, retry forwarding, or alerts.
        /// </summary>
        protected abstract Task HandleDeadLetterAsync(TPayload payload, int retryCount, CancellationToken token);


        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        { 
            _ctoken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        }

    }
}
