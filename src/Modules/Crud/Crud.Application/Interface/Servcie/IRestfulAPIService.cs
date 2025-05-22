using Shared.Domain.Models;
using Shared.Infrastructure.Observability;

namespace Crud.Application.Interface.Servcie
{
    public interface IRestfulAPIService<DTOType, DTOKeyType>
    {
        Task<bool> UpsertAsync(TraceId traceId, DTOType data, CancellationToken ct);
        Task<bool> DeleteAsync(TraceId traceId, DTOType keyContainer, CancellationToken ct);
        Task<IAsyncEnumerable<DTOType>> GetAllAsync(CancellationToken ct);
        Task<DTOType?> GetByIdAsync(DTOType keyContainer, CancellationToken ct);
        Task<CursorPageResponse<DTOType>> GetPagedAsync(CursorPageRequest<DTOType> request, CancellationToken ct);
    }
}
