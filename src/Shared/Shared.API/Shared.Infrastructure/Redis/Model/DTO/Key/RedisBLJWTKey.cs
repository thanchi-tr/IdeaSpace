using Shared.Domain.Constant.Redis;
using Shared.Infrastructure.Redis.Interface.Core;
using StackExchange.Redis;

namespace Shared.Infrastructure.Redis.Model.DTO.Key
{
    public class RedisBLJWTKey : IRedisSerialise
    {
        public string Jti { get; set; }
        public RedisKey ToRedisKey()
        {
            return new RedisKey($"{Prefix.JwtBlackList}:JWT:{Jti}");
        }
    }
}
