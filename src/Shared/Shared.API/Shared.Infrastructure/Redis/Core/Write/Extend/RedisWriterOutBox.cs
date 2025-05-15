 using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Shared.Infrastructure.Observability;
using Shared.Infrastructure.Redis.Config;
using Shared.Infrastructure.Redis.Interface.Core;
using Shared.Infrastructure.Redis.Interface.Extension.Recovery;
using Shared.Infrastructure.Redis.Model;
using Shared.Kernel.Interface.Health;
using Shared.Kernel.Observability.Logging.Constant;
using Shared.Kernel.Observability.Logging;
using System.Text.Json;
using System.Threading.Channels;
using StackExchange.Redis;
using Shared.Infrastructure.Redis.Interface.Extension.Operation;
using Serilog.Context;
using System.Collections.Concurrent;
using Shared.Infrastructure.Redis.Interface.Channel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Reflection;

namespace Shared.Infrastructure.Redis.Core.Write.Extend
{
    public class RedisWriterOutBox<KeyDTO, ValueDTO>
        : RedisWriter<KeyDTO, ValueDTO>,
        IRedisWriteOutBox<KeyDTO, ValueDTO>,
        IEnrichHealthCheck,
        IRecoveryHandler
        where KeyDTO : IRedisSerialise
    {
        /// Constant
        public const int INACTIVE = 0;
        public const int ACTIVE = 1;
        public const int SelfHealTriggerModulo = 0;

        private readonly ConcurrentDictionary<Guid, byte> _seen = new();
        private IChannel<RedisInstance<KeyDTO, ValueDTO>, KeyDTO, ValueDTO> _outboxChannel;
        private readonly ILogger _logger;
        private readonly Func<Task> _outageEscalationCB;
        private long _queueCount = 0;

        private volatile bool _isOutage;
        public bool IsOutage {  get { return _isOutage; } }
        /// Config:
        private int RetryInterval { get; init; }
        private int MaxRetryCount { get; init; }
        private int SelfHealInterval { get; init; }

        /// Limit the logging rate to prevent log pollutant
        public int OutBoxLogInterval { get; set; }


        /// Operation Flag
        /// Signal that the processing out box channel is occur
        private int _isWorkerActive = INACTIVE;

        public RedisWriterOutBox(
            IRedisConnectionManger conn,
            JsonSerializerOptions jsonOptions,
            ILogger logger,
            IConfiguration config,
            Func<Task> escalationCB,
            IServiceProvider provider,
            TraceId traceId
            ) : base(conn, jsonOptions)
        {
            var generalConfig = config.GetSection("AppMetaData:ModulesMDatas:Redis")
                    .Get<RedisConfig>();
            var CatastrophicConfig = config.GetSection("AppMetaData:ModulesMDatas:Redis")
                    .Get<CatastrophicHandlerConfig>();
            OutBoxLogInterval = config.GetSection("AppMetaData:ModulesMDatas:Redis")
                    .Get<RedisLog>()!.OutBoxLogInterval;
            RetryInterval = CatastrophicConfig.RetryInterval;
            MaxRetryCount = 5;// CatastrophicConfig.MaxRetryCount;
            SelfHealInterval = CatastrophicConfig.SelfHealInterval;
            // instantiate the register outbox reader and writer
            _outageEscalationCB = escalationCB;
            _logger = logger.InjectLoggerType(LoggerType.ModuleLog);

            // get the registed channel type
            // since the WriterOutBox is a singleton its channel must match scope
            _outboxChannel = provider.GetRequiredService<IChannel<RedisInstance<KeyDTO, ValueDTO>, KeyDTO, ValueDTO>>();
        }

        /// <summary>
        /// Intent usage:
        ///     1) Allow module to check its service health
        ///     2) contribute to decision to opt for alternative
        /// </summary>
        /// <param name="traceid">Empty - this writer have its own worker process who trigger log</param>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<HealthCheckResult> CheckHealthAsync(TraceId traceid, HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (this.IsConnectionHealthy())
            {
                return Task.FromResult(
                    HealthCheckResult.Healthy("Redis is connected", new Dictionary<string, object>
                    {
                        ["IsSchedulerRunning"] = _isWorkerActive == ACTIVE ? true : false,
                        ["OutboxQueueCount"] = Interlocked.Read(ref _queueCount),
                        ["RetryBackoff"] = RetryInterval,
                        ["MaxRetryCount"] = MaxRetryCount,
                        ["SelfHealModulo"] = SelfHealInterval
                    })
                );
            }
            return Task.FromResult(
                HealthCheckResult.Unhealthy("Redis connection is unavailable")
            );
        }

        /// <summary>
        /// Resolve in 2 path:
        ///     1- connection is clear, write to redis
        ///     2- connection not found/ redis down : send to outbox wait for issue resovle.
        /// </summary>
        /// <param name="traceId"> Allow inject module traceId</param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="ttl"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="RedisConnectionException"></exception>
        public async Task EnqueueAsync(TraceId traceId, KeyDTO key, ValueDTO value, TimeSpan ttl, CancellationToken ct = default)
        {
            if ((IsOutage))
            {
                return;
            }
            try
            {
                if (!IsConnectionHealthy())
                    throw new RedisConnectionException(ConnectionFailureType.ConnectionDisposed, "Connect is not reachable.");

                if (value is IExtractHashEntries extractable)
                {
                    await this.WriteHashAsync(key, extractable, ttl, ct);
                }
                else await this.WriteAsync(key, value, ttl, ct);

            }
            catch (Exception ex)
            {
                traceId.Refresh();
                using (LogContext.PushProperty("TraceId", traceId))
                {
                    _logger.Warning("RedisWriterOutBox: connection drop, Initiate retry every {RetryInterval}ms, {Exception}", RetryInterval, ex);
                }

                if( _outboxChannel.Writer.TryWrite(new RedisInstance<KeyDTO, ValueDTO> { Key = key, Value = value, TTL = ttl }))
                {
                    Interlocked.Increment(ref _queueCount);
                }
            }
            if(Interlocked.Read(ref _queueCount) == INACTIVE || Interlocked.CompareExchange(ref _isWorkerActive, 1, 0) == ACTIVE)
            {
                return;
            }
            _ = RetryAsync(traceId);
        }

        public async Task RetryAsync(TraceId traceId, CancellationToken ct = default)
        {
            if (IsOutage)
                return;

            var count = 0;

            var moduleName = Assembly.GetEntryAssembly()?.GetName().Name ?? ""; // if missing can leave blank, and opt for traceId

            traceId.Refresh();
            using (LogContext.PushProperty("TraceId", traceId))
            {
                while( !IsOutage &&
                    await _outboxChannel.Reader.WaitToReadAsync(ct) &&
                    count <= MaxRetryCount)
                {
                    if (!IsConnectionHealthy() && count % SelfHealInterval == SelfHealTriggerModulo)
                    {
                        _logger.Information("{Module}: Attempt retry: {RetryCount}/{MaxRetry}", moduleName, count + 1, MaxRetryCount);
                        this.AttemptHeal();
                    }

                    if(IsConnectionHealthy())
                    {
                        await foreach (var msg in _outboxChannel.Reader.ReadAllAsync(ct))
                        {
                            Interlocked.Decrement(ref _queueCount);
                            // Handle retry logic here
                            await AttemptProcessOutboxMsg(msg, ct);
                        }
                    }
                    
                    await Task.Delay(RetryInterval, ct);
                    count++;
                }
                Interlocked.Exchange(ref _isWorkerActive, INACTIVE);
                if (count > MaxRetryCount)
                {
                    _logger.Warning("{Module}: Redis not recoverable after {MaxRetryCount} attempts. {outBoxCount} items remain unprocessed. Attempt Escalation.",
                        moduleName, count, Interlocked.Read(ref _queueCount));
                    _isOutage = true;
                    await _outageEscalationCB();
                    return;
                }
                _logger.Information("{Module}: Retry Success: Redis recovered after {Attempts} attempts. Queue flushed.", moduleName, count, _queueCount);

            }
        }

        /// Private Helper:
        private async Task AttemptProcessOutboxMsg(RedisInstance<KeyDTO, ValueDTO> msg, CancellationToken ct = default)
        {
            if(!IsConnectionHealthy())
            {
                if(_outboxChannel.Writer.TryWrite(msg))
                    Interlocked.Increment(ref _queueCount);
                return;
            }
            try 
            { 
                if(msg.Key is IExtractHashEntries extractable)
                {
                    await this.WriteHashAsync(msg.Key, extractable, msg.TTL, ct);
                }
                else await this.WriteAsync(msg.Key, msg.Value, msg.TTL, ct);

               
            }
            catch (Exception ex)
            {
                // Log is still wrapped under context of Retry block
                _logger.Warning("Issue raise after connection revised");
                if(_outboxChannel.Writer.TryWrite(msg))
                    Interlocked.Increment(ref _queueCount);
                return ;
            }

            return ;
        }

        public void OutageResovle()
        {
            _isOutage = false;

        }
    }
}
