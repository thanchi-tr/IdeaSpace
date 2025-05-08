using Shared.Infrastructure.Redis.Interface.Core;
using StackExchange.Redis;

namespace Shared.Infrastructure.Redis.Model.DTO.Key
{
    /// <summary>
    /// wil return
    /// </summary>
    public class RedisGatewayRateLimitBucketKey : IRedisSerialise
    {
        public Guid UserId { get; set; }

        public RedisKey ToRedisKey()
        {
            return new RedisKey($"ApiGateWay:RateLimit:{UserId}");
        }
    }
}
