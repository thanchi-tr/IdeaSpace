using RabbitMQ.Client;
namespace Shared.Messaging.Interface
{
    public interface IRabbitMqConnectionManager : IDisposable
    {
        Task<IConnection> GetConnectionAsync();
    }
}
