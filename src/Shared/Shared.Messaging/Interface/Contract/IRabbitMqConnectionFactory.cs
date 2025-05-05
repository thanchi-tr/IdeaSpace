using RabbitMQ.Client;

namespace IdeaSpace.Infrastructure.Interface.Contract
{
    /// <summary>
    /// Enable DI to create a singleton service and let it inject every where
    /// </summary>
    public interface IRabbitMqConnectionFactory
    {
        Task<IConnection> CreateConnectionAsync();
    }
}
