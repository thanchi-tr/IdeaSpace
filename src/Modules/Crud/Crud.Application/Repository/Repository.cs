using Crud.Application.Interface.Repository;
using Crud.Application.Interface.System;
using Crud.Application.Interface.Util;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Models;
using Shared.Infrastructure.Observability;
using Shared.Infrastructure.Redis.Interface.Channel;
using Shared.Infrastructure.Redis.Interface.Core;
using Shared.Kernel.Observability.Logging;
using Shared.Kernel.Observability.Logging.Constant;
using System.Linq;
using System.Linq.Expressions;

namespace Crud.Application.Repository
{

    public class Repository<DTOType, ORMType, DTOKeyType> : IRepository<DTOType, DTOKeyType>
        where DTOKeyType : class
        where DTOType : IRedisSerialise, IHasKey<DTOKeyType>, new()
        where ORMType : class
    {
        private readonly TraceId _traceId;
        private readonly Reader<DTOType, ORMType> _systemReader;
        private readonly Dictionary<LoggerType, Serilog.ILogger> _loggers;
        private readonly IMap _mapper; // custom mapper, response to auto mapper go subscription-base service
        
        public Repository(TraceId traceId, Reader<DTOType, ORMType> systemReader, Serilog.ILogger logger, IMap mapper)
        {
            _traceId = traceId;
            _systemReader = systemReader;
            _loggers = logger.Split();
            _mapper = mapper;
        }

        /// <summary>
        /// This method by pass redis and query db straight, wont trigger cache event
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IAsyncEnumerable<DTOType>> GetAllAsync(CancellationToken ct)
        {
            var query = await _systemReader.GetAllAsync(); // await the Task<IQueryable<ORMType>>

            return query
                .Select(orm => _mapper.Map<ORMType, DTOType>(orm))
                .AsAsyncEnumerable(); // Convert to IAsyncEnumerable
        }

        public async Task<IAsyncEnumerable<DTOType>> GetAllAsync(Expression<Func<DTOType, bool>> predicate, CancellationToken ct)
        {
            var query = await _systemReader.GetAllAsync(); // await the Task<IQueryable<ORMType>>

            return query
                .Select(orm => _mapper.Map<ORMType, DTOType>(orm))
                .Where(predicate)
                .AsAsyncEnumerable(); // Convert to IAsyncEnumerable
        }

        public async Task<DTOType?> GetByIdAsync(DTOType Id, CancellationToken ct)
        {
            var target = await _systemReader.GetAsync(traceId: _traceId, key:Id, ct);
            if (target == null)
            {
                _loggers[LoggerType.ModuleLog].Warning("Object with key {Key} not found in cache or DB", Id);
                return default;
            }
            return _mapper.Map<ORMType, DTOType>(target!);
        }

        public async Task<CursorPageResponse<DTOType>> GetPagedAsync(
            CursorPageRequest<DTOType> request, 
            CancellationToken ct)
        {
            throw new NotImplementedException();

        }

        public Task<bool> OptimisticDeleteAsync(TraceId traceId, DTOKeyType key, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<bool> OptimisticUpsertAsync(TraceId traceId, DTOType data, CancellationToken ct)
        {
            throw new NotImplementedException();
        }


    }
}
