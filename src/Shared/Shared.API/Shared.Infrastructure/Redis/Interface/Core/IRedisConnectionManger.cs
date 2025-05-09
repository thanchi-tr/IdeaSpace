using StackExchange.Redis;

namespace Shared.Infrastructure.Redis.Interface.Core
{
    public interface IRedisConnectionManger : ISelfHeal
    {
        IDatabase GetDatabase(int db);
        ISubscriber GetSubscriber();
        IServer GetServer(string host, int port);
        IConnectionMultiplexer GetConnection();

        Task CloseAsync();
    }
}
