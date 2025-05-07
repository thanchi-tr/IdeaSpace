using StackExchange.Redis;

namespace Shared.Infrastructure.Redis.Interface.Core
{
    public interface IRedisSerialise
    {
        RedisKey ToRedisKey();
    }
}
