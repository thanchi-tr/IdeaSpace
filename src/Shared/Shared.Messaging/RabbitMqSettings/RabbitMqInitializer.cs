using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
namespace IdeaSpace.Infrastructure;

public class RabbitMqInitializer
{
    private readonly ILogger<RabbitMqInitializer> _logger;
    private readonly IConfiguration _configuration;

    public RabbitMqInitializer(ILogger<RabbitMqInitializer> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task Initialize()
    {
        var factory = new RabbitMQ.Client.ConnectionFactory
        {
            HostName = _configuration["RabbitMq:HostName"] ?? "localhost",
            UserName = _configuration["RabbitMq:UserName"] ?? "admin",
            Password = _configuration["RabbitMq:Password"] ?? "admin",
            Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
        };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        const string exchangeName = "idea.exchange";
        const string queueName = "idea.created.queue";
        const string deadLetterExchange = "idea.dlx";
        const string deadLetterQueue = "idea.created.dead.letter.queue";

        // Main exchange
        await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Topic, durable: true);

        // Dead-letter exchange and queue
        await channel.ExchangeDeclareAsync(exchange: deadLetterExchange, type: ExchangeType.Fanout, durable: true);
        await channel.QueueDeclareAsync(queue: deadLetterQueue, durable: true, exclusive: false, autoDelete: false);
        await channel.QueueBindAsync(queue: deadLetterQueue, exchange: deadLetterExchange, routingKey: "");

        // Main queue with DLX (Dead Letter Exchange)
        var args = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", deadLetterExchange }
        };

        await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: args);
        await channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: "#");

        _logger.LogInformation("✅ RabbitMQ infrastructure initialized successfully.");
    }
}
