using Microsoft.EntityFrameworkCore.Storage;
using StackExchange.Redis;

namespace Shared.Infrastructure.Redis
{
    public interface IRedisConnectionManger
    {
        StackExchange.Redis.IDatabase GetDatabase(int db);
        ISubscriber GetSubscriber();
        IServer GetServer(string host, int port);
        IConnectionMultiplexer GetConnection();
    }
}
