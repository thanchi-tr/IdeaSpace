using StackExchange.Redis;
namespace Shared.Infrastructure.Redis.Interface.Core.Redis
{
    public interface IWrite<KeyDTO, ValueDTO>
    {
        Task WriteAsync(KeyDTO key, ValueDTO value, TimeSpan ttl, CancellationToken ct=default);
    }
}
