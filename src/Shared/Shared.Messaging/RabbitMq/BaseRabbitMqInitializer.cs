using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Shared.Messaging.Interface;
using Shared.Domain.Constant.RabbitMQ;
using Shared.Messaging.RabbitMq;
namespace IdeaSpace.Infrastructure;

/// <summary>
/// The base class come with exchange for dead letter queue
/// as well as the the queue for Recover system (which most module will make use off)
/// </summary>
public abstract class BaseRabbitMqInitializer
{
    private readonly ILogger<BaseRabbitMqInitializer> _logger;
    private readonly IConfiguration _configuration;
    private readonly IRabbitMqConnectionManager _connectionManager;
    // the strategy will  include all the config of queue, dlx if have, and abstracted it away from the initialize.
    // we want to use the strategy because we want a standardise setting as oppose to manually config every single module
    public BaseRabbitMqInitializer(
        ILogger<BaseRabbitMqInitializer> logger, 
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
    /// Later on, we can implement the strategy for handling event flow of RabbitMQ exchange,
    // and plugin during the pre initialize stage (this method) so that the 
    /// </summary>
    /// <returns>nothing</returns>
    public virtual async Task<IChannel> Initialize()
     {
        using var connection = await _connectionManager.GetConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        // Main exchange - compose of health and recover
        await channel.ExchangeDeclareAsync(Exchange.HEALTH, type: ExchangeType.Topic, durable: true);
        await channel.ExchangeDeclareAsync(Exchange.DLX, type: ExchangeType.Topic, durable: true);

        // declare a queue for  check health
        await channel.QueueDeclareAsync(
            Exchange.HEALTH, 
            durable: false, 
            autoDelete: true, 
            exclusive: true,
            arguments: new Dictionary<string, object?>
            {
                ["x-expires"] = 30_000
            });
        await channel.QueueBindAsync(
            Exchange.HEALTH, 
            Exchange.HEALTH, 
            routingKey: Exchange.HEALTH);
        _logger.LogInformation("RabbitMQ infrastructure (Recover, Health) initialized successfully.");
        return channel;
    }
}
