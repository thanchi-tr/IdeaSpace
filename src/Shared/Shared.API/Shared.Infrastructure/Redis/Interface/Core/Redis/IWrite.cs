using Shared.Infrastructure.Redis.Interface.Extension.Operation;
using Shared.Kernel.Interface.Health;
using StackExchange.Redis;
namespace Shared.Infrastructure.Redis.Interface.Core.Redis
{
    public interface IWrite<KeyDTO, ValueDTO> : ISelfHeal, IHotSwapConnectionPath
    {
        Task WriteAsync(KeyDTO key, ValueDTO value, TimeSpan ttl, CancellationToken ct=default);
        Task WriteHashAsync(KeyDTO key, IExtractHashEntries value, TimeSpan ttl, CancellationToken ct = default);
    }
}
