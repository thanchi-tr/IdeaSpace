using Shared.Infrastructure.Redis.Interface.Core;
using StackExchange.Redis;

namespace Shared.Infrastructure.Redis.Model.DTO.Key
{
    public class RedisCollectionKey : ObservableDTO, IRedisSerialise
    {
        public Guid CollectionId { get; set; }

        public RedisKey ToRedisKey()
        {
            return new RedisKey($"GateKeeper:Collection:{TraceId}:{BatchId}:{CollectionId}");
        }
    }
}
