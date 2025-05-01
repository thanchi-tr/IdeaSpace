using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Shared.Messaging.Interface;
using Shared.Domain.Constant.RabbitMQ;
namespace IdeaSpace.Infrastructure;

public class RabbitMqInitializer
{
    private readonly ILogger<RabbitMqInitializer> _logger;
    private readonly IConfiguration _configuration;
    private readonly IRabbitMqConnectionManager _connectionManager;
    public RabbitMqInitializer(
        ILogger<RabbitMqInitializer> logger, 
        IConfiguration configuration,
        IRabbitMqConnectionManager connectionManager)
    {
        _logger = logger;
        _configuration = configuration;
        _connectionManager = connectionManager;
    }

    /// <summary>
    /// Initiate the shared infrastructure of the rabit Mq
    /// , the recover (expect to be accessible by all module) 
    /// health (check responsive of Rabbit)
    /// </summary>
    /// <returns>nothing</returns>
    public async Task Initialize()
    {
        using var connection = await _connectionManager.GetConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        // Main exchange - compose of health and recover
        await channel.ExchangeDeclareAsync(Exchange.HEALTH,type: ExchangeType.Topic, durable: true);
        await channel.QueueDeclareAsync(Queue.HEALTH_CHECK, durable: true, exclusive: false, autoDelete: false);

        await channel.ExchangeDeclareAsync(Exchange.RECOVER, type: ExchangeType.Topic, durable: true);
        await channel.ExchangeDeclareAsync(Exchange.RECOVER_DLX, type: ExchangeType.Topic, durable: true);
        
        // Main queue with DLX (Dead Letter Exchange)
        var args = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", Exchange.RECOVER_DLX }
        };
        await channel.QueueDeclareAsync(Queue.DELTA_LOG, durable: true, exclusive: false, autoDelete: false, arguments: args);
        await channel.QueueBindAsync(Queue.DELTA_LOG_DLQ, exchange: Exchange.RECOVER_DLX, routingKey: "#");
        await channel.QueueDeclareAsync(Queue.RECOVER_COMMAND, durable: true, exclusive: false, autoDelete: false, arguments: args);
        await channel.QueueBindAsync(Queue.RECOVER_COMMAND_DLQ, exchange: Exchange.RECOVER_DLX, routingKey: "#");
        await channel.QueueDeclareAsync(Queue.RECOVER_CLEAN, durable: true, exclusive: false, autoDelete: false, arguments: args);
        await channel.QueueBindAsync(Queue.RECOVER_CLEAN_DLQ, exchange: Exchange.RECOVER_DLX, routingKey: "#");

        _logger.LogInformation("RabbitMQ infrastructure (Recover, Health) initialized successfully.");
    }
}
