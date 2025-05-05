using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Shared.Messaging.Interface;
using Shared.Messaging.Interface.Contract;

namespace Shared.Messaging
{
    public class RabbitMqConnectionManager : IRabbitMqConnectionManager
    {
        private readonly ILogger<RabbitMqConnectionManager> _logger;
        private readonly IOptions<RabbitMqOptions> _opts;
        private readonly Lazy<Task<IConnection>> _connection;
        private bool _disposed;

        public RabbitMqConnectionManager(
            ILogger<RabbitMqConnectionManager> logger,
            IOptions<RabbitMqOptions> options)
        {
            _logger = logger;
            _opts = options;
            _connection = new Lazy<Task<IConnection>>(ConnectAsync, true);
        }

        private async Task<IConnection> ConnectAsync()
        {
            _logger.LogInformation("Initializing RabbitMQ connection to {Host}", _opts.Value.HostName);

            var factory = new ConnectionFactory
            {
                HostName = _opts.Value.HostName,
                UserName = _opts.Value.UserName,
                Password = _opts.Value.Password,
                Port = int.Parse(_opts.Value.Port)
            };
            try
            {
                return await factory.CreateConnectionAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create RabbitMQ connection.");
                throw;
            }
        }
        public void Dispose()
        {
            if (_disposed) return;

            if (_connection.IsValueCreated && _connection.Value.IsCompletedSuccessfully)
            {
                _connection.Value.Result?.Dispose();
                _logger.LogInformation("RabbitMQ connection disposed.");
            }

            _disposed = true;
        }

        public Task<IConnection> GetConnectionAsync()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(RabbitMqConnectionManager));

            return  _connection.Value;
        }
    }
}
