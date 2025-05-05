using IdeaSpace.Infrastructure.Interface.Contract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Shared.Messaging.Interface.Contract;

namespace IdeaSpace.Infrastructure.Messaging.RabbitMq;

public class RabbitMqConnectionFactory : IRabbitMqConnectionFactory
{
    private readonly IConfiguration _configuration;
    private IConnection _connection;
    private readonly IOptions<RabbitMqOptions> _opts;
    public RabbitMqConnectionFactory(IConfiguration configuration, IOptions<RabbitMqOptions> opts)
    {
        _configuration = configuration;
        _opts = opts;
    }

    public async Task<IConnection> CreateConnectionAsync()
    {
        if (_connection != null && _connection.IsOpen)
            return _connection;

        var factory = new ConnectionFactory
        {
            HostName = _opts.Value.HostName ?? "localhost",
            UserName = _opts.Value.UserName ?? "admin",
            Password = _opts.Value.Password ?? "admin"
        };

        _connection = await factory.CreateConnectionAsync();
        return _connection;
    }
}
