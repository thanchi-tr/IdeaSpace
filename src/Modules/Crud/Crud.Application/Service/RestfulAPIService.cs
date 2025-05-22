
using Crud.Application.Interface.Repository;
using Crud.Application.Interface.Servcie;
using Shared.Domain.Models;
using Shared.Infrastructure.Observability;
using Shared.Infrastructure.Redis.Interface.Channel;
using Shared.Infrastructure.Redis.Interface.Core;

namespace Crud.Application.Service
{
    public class RestfulAPIService<ORMType, DTOType, DTOKeyType> : IRestfulAPIService<DTOType, DTOKeyType>
        where DTOKeyType : class
        where DTOType : IRedisSerialise, IHasKey<DTOKeyType>, new()
        where ORMType : class
    {
        private readonly IRepository<DTOType, DTOKeyType> _repo;

        public RestfulAPIService(IRepository<DTOType, DTOKeyType> repo)
        {
            _repo = repo;
        }

        public Task<bool> DeleteAsync(TraceId traceId, DTOType keyContainer, CancellationToken ct)
        {
            return _repo.OptimisticDeleteAsync(traceId, keyContainer, ct);
        }

        public Task<IAsyncEnumerable<DTOType>> GetAllAsync(CancellationToken ct)
        {
            return _repo.GetAllAsync(ct);
        }

        public Task<DTOType?> GetByIdAsync(DTOType keyContainer, CancellationToken ct)
        {
            return _repo.GetByIdAsync(keyContainer, ct);
        }

        public Task<CursorPageResponse<DTOType>> GetPagedAsync(CursorPageRequest<DTOType> request, CancellationToken ct)
        {
            return _repo.GetPagedAsync(request, ct);
        }

        public Task<bool> UpsertAsync(TraceId traceId, DTOType data, CancellationToken ct)
        {
            return _repo.OptimisticUpsertAsync(traceId, data, ct);
        }
    }
}
