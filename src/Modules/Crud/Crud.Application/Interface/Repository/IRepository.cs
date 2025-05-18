using Shared.Domain.Models;
using Shared.Infrastructure.Observability;
using Shared.Infrastructure.Redis.Interface.Channel;
using System.Linq.Expressions;

namespace Crud.Application.Interface.Repository
{
    /// <summary>
    /// A bridge into infrastructure layer.
    /// </summary>
    /// <typeparam name="DTOType"></typeparam>
    /// <typeparam name="ORMType"></typeparam>
    /// <typeparam name="DTOKeyType"></typeparam>
    /// <remarks>
    ///     Repository must not expose ORM Type object.
    ///     Must convert it into DTO Type object.
    /// </remarks>
    public interface IRepository<DTOType, DTOKeyType>
        where DTOType : IHasKey<DTOKeyType>
    {
        /// <summary>
        /// Upserts a record based on the presence of the key.
        /// </summary>
        /// <param name="traceId">Unique identifier for tracing the operation.</param>
        /// <param name="data">The DTO mapped to the target ORM model.</param>
        /// <param name="ct">Cancellation token to propagate cancellation signals.</param>
        /// <returns>Returns true if the operation completed without exceptions.</returns>
        /// <remarks>
        /// See Shared.Infrastructure.Observability.TraceId for how trace context is composed.
        /// </remarks>
        Task<bool> OptimisticUpsertAsync(TraceId traceId, DTOType data, CancellationToken ct);

        /// <summary>
        /// Optimistically delete object base of key
        /// </summary>
        /// <param name="traceId"> refer to Shared.Infrastructure.Observability.TraceId for implementation detail, ClientId compose of UserId:DeviceId:JWT <- for enrich logging context</param>
        /// <param name="ct">Built in kill switch</param>
        /// <returns></returns>
        /// <remarks>
        /// See Shared.Infrastructure.Observability.TraceId for how trace context is composed.
        /// </remarks>
        Task<bool> OptimisticDeleteAsync(TraceId traceId, DTOKeyType key, CancellationToken ct);

        /// <summary>
        /// Generic get all
        /// </summary>
        /// <param name="ct">Built in kill switch</param>
        /// <returns></returns>
        Task<IAsyncEnumerable<DTOType>> GetAllAsync( CancellationToken ct);

        /// <summary>
        /// Retrieves a filtered set of records based on the provided predicate.
        /// </summary>
        /// <param name="predicate">
        /// A LINQ-compatible expression used to filter ORM records.
        /// When backed by Redis, this predicate is translated to Redis-specific query logic, 
        /// enabling efficient filtering without full in-memory projection.
        /// </param>
        /// <param name="ct">Cancellation token for cooperative cancellation.</param>
        /// <returns>
        /// A stream of matching <typeparamref name="ORMType"/> objects that satisfy the predicate.
        /// </returns>
        /// <remarks>
        /// This method is optimized for Redis-based filtering using expression tree translation.
        /// Performance is dependent on the Redis data model and predicate complexity.
        /// </remarks>
        Task<IAsyncEnumerable<DTOType>> GetAllAsync(Expression<Func<DTOType, bool>> predicate, CancellationToken ct);

        /// <summary>
        /// Get 1 object if exist
        /// or null if none exist
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ct">Built in kill switch</param>
        /// <returns></returns>
        Task<DTOType?> GetByIdAsync(DTOType Id, CancellationToken ct);

        /// <summary>
        /// Get n number of ORMType class base off of config define in CursorPaseRequest
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <remarks>
        ///     Refer to Shared.Domain.Models.CursorPageRequest for detail
        /// </remarks>
        Task<CursorPageResponse<DTOType>> GetPagedAsync(CursorPageRequest<DTOType> request, CancellationToken ct);
    }
}
