using Shared.Kernel.Interface.Health;
using StackExchange.Redis;

namespace Shared.Infrastructure.Redis.Interface.Core
{
    public interface IRedisConnectionManger : ISelfHeal, IHotSwapConnectionPath
    {
        IDatabase GetDatabase(int db);
        ISubscriber GetSubscriber();
        IServer GetServer(string host, int port);
        IConnectionMultiplexer GetConnection();

        Task CloseAsync();
    }
}
