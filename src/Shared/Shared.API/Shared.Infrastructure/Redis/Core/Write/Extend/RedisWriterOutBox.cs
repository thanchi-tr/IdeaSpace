using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Context;
using Shared.Infrastructure.Observability;
using Shared.Infrastructure.Redis.Interface.Core;
using Shared.Infrastructure.Redis.Interface.Extension.Operation;
using Shared.Infrastructure.Redis.Interface.Extension.Recovery;
using Shared.Infrastructure.Redis.Model;
using Shared.Kernel.GeneralConfig;
using Shared.Kernel.Observability.Logging;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;

namespace Shared.Infrastructure.Redis.Core.Write.Extend
{
    /// <summary>
    /// This is an singleton that will live as long as the application.
    /// 
    /// </summary>
    /// <typeparam name="KeyDTO"></typeparam>
    /// <typeparam name="ValueDTO"></typeparam>
    public class RedisWriterOutBox<KeyDTO, ValueDTO> 
        : RedisWriter<KeyDTO, ValueDTO>, IRedisWriteOutBox<KeyDTO, ValueDTO>
        where KeyDTO : IRedisSerialise
    {
        private readonly ConcurrentQueue<RedisInstance<KeyDTO, ValueDTO>> _queue;
        private readonly int _maxCapacity;
        private bool _isSchedulerRun = false;


        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly ILogger _logger;
        private readonly ModuleMetaData _moduleMetaData;
        public readonly int RETRY_INTERVAL = 100;
        private readonly int MaxRetryCount;// default total time 30s 
        private readonly Func<Task> _redisHealingEscalationCB;
        public RedisWriterOutBox(
            IRedisConnectionManger conn, 
            JsonSerializerOptions jsonOptions, 
            ILogger logger,
            IConfiguration config,
            Func<Task>  escalationCB
            ) : base(conn, jsonOptions)
        {
            _queue = new ConcurrentQueue<RedisInstance<KeyDTO, ValueDTO>>();
            // This failure point will be report in module event
            _logger = logger.ForContext("Type", LoggerType.ModuleLog);
            _maxCapacity = config.GetSection("AppMetaData:ModulesMDatas:Redis:MaxRetryCapacity").Get<int>();
            _moduleMetaData = config
                .GetSection("AppMetaData:ModulesMDatas:Redis")
                .Get<ModuleMetaData>() ?? new ModuleMetaData { IssuerId = Guid.NewGuid(), IssuerType = Observability.IssuerType.Internal };
            var retryCount = config
                .GetSection("AppMetaData:ModulesMDatas:RedisConfig:MaxRetry")
                .Get<int>();
            MaxRetryCount = retryCount == 0 
                ? 300 // default to 30s total attempt wait for revolving
                : retryCount;
            _redisHealingEscalationCB = escalationCB;
        }

        public async Task EnqueueAsync(KeyDTO key, ValueDTO value, TimeSpan ttl, CancellationToken ct = default)
        {
            try
            {
                await _semaphore.WaitAsync(ct);
                try
                {
                    if(value is IExtractHashEntries extractable)
                    {
                        await this.WriteHashAsync(key, extractable, ttl, ct);
                    }
                    await this.WriteAsync(key, value, ttl, ct);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            catch (RedisConnectionException ex)
            {
                using (LogContext.PushProperty("TraceId", new TraceId(
                    issuerId: _moduleMetaData.IssuerId,
                    issuerType: _moduleMetaData.IssuerType )
                ))
                {
                    var moduleName = Assembly.GetEntryAssembly()?.GetName().Name ?? ""; // if missing can leave blank, and opt for traceId
                    _logger.Error($"{moduleName} Redis connection drop, Initiate retry every {RETRY_INTERVAL}ms");
                }
                // Cache stop functioning ashort while only yield minimal performance downgrade <- request db.
                // we wont expect redis connection to drop (else) we (human technician) need to intervent anyway
                if(_queue.Count <  MaxRetryCount)
                    _queue.Enqueue(new RedisInstance<KeyDTO, ValueDTO> { Key = key, Value = value, TTL = ttl });
                await _semaphore.WaitAsync(ct);
                try
                {
                    if (!_isSchedulerRun)
                        _isSchedulerRun = true;
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }

        public async Task RetryAsync(CancellationToken ct = default)
        {
            var count = 0; 
            var moduleName = Assembly.GetEntryAssembly()?.GetName().Name ?? ""; // if missing can leave blank, and opt for traceId

            // we add the retry number later
            using (LogContext.PushProperty("TraceId", new TraceId(
                    issuerId: _moduleMetaData.IssuerId,
                    issuerType: _moduleMetaData.IssuerType)
                ))
            {
                while (_isSchedulerRun && !_queue.IsEmpty && count <= MaxRetryCount)
                {
                    await _semaphore.WaitAsync(ct);
                    try
                    {
                        _logger.Warning("{Module}: Attempt retry: {RetryCount}/{MaxRetry}", moduleName, count + 1, MaxRetryCount);

                        this.AttemptHeal();
                        if(this.IsConnectionHealthy() &&  _queue.TryDequeue(out var redisInstance))
                            await this.EnqueueAsync(redisInstance.Key, redisInstance.Value, redisInstance.TTL, ct);
                        else
                            _isSchedulerRun = false;
                        count++;
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                    await Task.Delay(RETRY_INTERVAL, ct);
                }
                if(count >  MaxRetryCount)
                {
                    _logger.Warning("{Module}: Redis not recoverable after {MaxRetryCount} attempts. {Remaining} items remain unprocessed. Attempt Escalation.",moduleName, _queue.Count);
                    await _redisHealingEscalationCB();
                    return;
                }
                _logger.Information("{Module}: Retry Success: Redis recovered after {Attempts} attempts. Queue flushed.", moduleName, count);
            }
        }
    }
}
