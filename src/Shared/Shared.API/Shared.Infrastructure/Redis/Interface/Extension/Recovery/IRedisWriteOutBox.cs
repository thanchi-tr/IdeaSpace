using Shared.Infrastructure.Redis.Interface.Core.Redis;

namespace Shared.Infrastructure.Redis.Interface.Extension.Recovery
{
    public interface IRedisWriteOutBox<KeyDTO, ValueDTO> : IWrite<KeyDTO, ValueDTO>
    {
        Task EnqueueAsync(KeyDTO key, ValueDTO value, TimeSpan ttl, CancellationToken ct = default);
        Task RetryAsync(CancellationToken ct = default);
    }
}
