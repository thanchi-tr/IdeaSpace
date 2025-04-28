using IdeaSpace.Infrastructure.Interface.MessageBroker;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace IdeaSpace.Infrastructure.Messaging.RabbitMq;

public class RabbitMqConnectionFactory : IRabbitMqConnectionFactory
{
    private readonly IConfiguration _configuration;
    private IConnection _connection;

    public RabbitMqConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<IConnection> CreateConnectionAsync()
    {
        if (_connection != null && _connection.IsOpen)
            return _connection;

        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMq:HostName"] ?? "localhost",
            UserName = _configuration["RabbitMq:UserName"] ?? "admin",
            Password = _configuration["RabbitMq:Password"] ?? "admin"
        };

        _connection = await factory.CreateConnectionAsync();
        return _connection;
    }
}
