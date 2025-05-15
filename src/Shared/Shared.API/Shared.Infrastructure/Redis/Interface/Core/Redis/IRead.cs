using StackExchange.Redis;

namespace Shared.Infrastructure.Redis.Interface.Core.Redis
{
    public interface IRead<KeyDTO,ValueDTO>
    {
        Task<ValueDTO?> ReadAsync(KeyDTO key, CancellationToken ct = default);
        Task<HashEntry[]?> ReadHashAsync(KeyDTO key, CancellationToken ct = default);
    }
}
