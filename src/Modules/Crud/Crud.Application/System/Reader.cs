using Serilog;
using Serilog.Context;
using Shared.Domain.Constant.RabbitMQ.Type;
using Shared.Infrastructure.Data;
using Shared.Infrastructure.Observability;
using Shared.Infrastructure.Redis.Core.Read;
using Shared.Infrastructure.Redis.Interface.Core;
using Shared.Kernel.Observability.Logging;
using Shared.Kernel.Observability.Logging.Constant;
using Shared.Kernel.Util.Intergrity;
using Shared.Messaging.Constanst.Contract;
using Shared.Messaging.Interface.Publisher;
using System.Text.Json;

namespace Crud.Application.Interface.System
{
    public class Reader<KeyDTO, ORMType> : ISystemRead<ORMType, KeyDTO>
    
        where KeyDTO : IRedisSerialise
        where ORMType : class
    {
        private RedisReader<KeyDTO, ORMType> _cache { get; set; }
        private ReadOnlyAppSqlDbContext _readonlyDb {  get; set; }

        private Dictionary<LoggerType,Serilog.ILogger> _logger;
        private RabbitMqMessagePublisher<BaseEvent<ORMType, CacheEventType>> _publisher;
        public Reader(
            RedisReader<KeyDTO, ORMType> cache, 
            ReadOnlyAppSqlDbContext readonlyDb,
            IMessagePublisher<BaseEvent<ORMType, CacheEventType>> messagePublisher,
            ILogger logger)
        {
            _cache = cache;
            _readonlyDb = readonlyDb;
            _publisher = (RabbitMqMessagePublisher<BaseEvent<ORMType, CacheEventType>>?)
                messagePublisher!;
            _logger = logger.Split();

        }

        /// <summary>
        /// Reader try to get using the following order
        ///     1. redis cache
        ///     2. sql db
        /// </summary>
        /// <param name="traceId"></param>
        /// <param name="key"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<ORMType?> GetAsync(TraceId traceId, KeyDTO key, CancellationToken ct=default)
        {
            traceId.Refresh();
            using (LogContext.PushProperty("TraceId", traceId))
            {
                // Look in redis for the item
                ORMType? target = default(ORMType);
                try
                {
                    string serialisedVal;
                    target = await _cache.ReadAsync(key, ct);
                    if (target != null)
                    {
                        serialisedVal = JsonSerializer.Serialize(target, JsonSerializerStableOptions.Object);
                        // publish cache miss event
                        // this will publish into cache.operate.lazy.queue 
                        await _publisher.PublishAsync(new BaseEvent<ORMType, CacheEventType>(
                            correlationId: traceId.ToString(),
                            payload: target!,
                            changeType: CacheEventType.CacheHit,
                            checksum: serialisedVal.ComputeChecksum()
                        ));
                        
                        return target;
                    }
                    target = await _readonlyDb.Set<ORMType>() // built in no tracking
                        .FindAsync(key, ct);

                    serialisedVal = JsonSerializer.Serialize(target, JsonSerializerStableOptions.Object);
                    // this will publish into cache.operate.lazy.queue 
                    await _publisher.PublishAsync(new BaseEvent<ORMType, CacheEventType>(
                        correlationId: traceId.ToString(),
                        payload: target!,
                        changeType:CacheEventType.CacheMiss,
                        checksum: serialisedVal.ComputeChecksum()
                    ));
                    return target;
                }
                catch (Exception ex) // ensure system does not crash on fail to handle low level, push it to dynamic log instead
                {
                    _logger[LoggerType.ModuleLog].Error("SystemRead:Get Fail with {Exception}", ex.Message);
                }
                return target;
            }

        }

        public Task<IQueryable<ORMType>> GetAllAsync()
        {
            return Task.FromResult(_readonlyDb.Set<ORMType>().AsQueryable());
        }
    }
}
