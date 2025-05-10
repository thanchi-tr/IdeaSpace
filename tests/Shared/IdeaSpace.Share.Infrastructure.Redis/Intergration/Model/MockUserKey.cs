using Shared.Infrastructure.Redis.Interface.Core;
using StackExchange.Redis;

namespace IdeaSpace.Share.Infrastructure.Redis.Intergration.Model
{
    public class MockUserKey : IRedisSerialise
    {
        public string UserKey { get; set; }
        public RedisKey ToRedisKey()
        {
            return new RedisKey($"Test:{UserKey}");
        }
    }
}
