using Shared.Infrastructure.Observability;

namespace Crud.Application.Interface.System
{
    public interface ISystemRead<ORMType, DTOKeyType>
    {
        Task<ORMType?> GetAsync(TraceId traceId, DTOKeyType key, CancellationToken ct);
        Task<IQueryable<ORMType>> GetAllAsync();
    }
}
