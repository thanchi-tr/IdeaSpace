namespace Shared.Infrastructure.Redis.Interface.Core.Redis
{
    public interface IRead<KeyDTO,ValueDTO>
    {
        Task<ValueDTO?> ReadAsync(KeyDTO key, CancellationToken ct = default);
    }
}
