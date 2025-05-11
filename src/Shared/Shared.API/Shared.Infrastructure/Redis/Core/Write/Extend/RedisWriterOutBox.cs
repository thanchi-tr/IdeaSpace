using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Context;
using Shared.Infrastructure.Observability;
using Shared.Infrastructure.Redis.Interface.Core;
using Shared.Infrastructure.Redis.Interface.Extension.Operation;
using Shared.Infrastructure.Redis.Interface.Extension.Recovery;
using Shared.Infrastructure.Redis.Model;
using Shared.Kernel.GeneralConfig;
using Shared.Kernel.Interface.Health;
using Shared.Kernel.Observability.Logging;
using Shared.Kernel.Observability.Logging.Constant;
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
        : RedisWriter<KeyDTO, ValueDTO>, 
        IRedisWriteOutBox<KeyDTO, ValueDTO>,
        IEnrichHealthCheck
        where KeyDTO : IRedisSerialise
    {
        private int _isSchedulerRun = 0;
        private long _queueCount = 0;
        private const int LogInterval = 20;
        private readonly ConcurrentQueue<KeyDTO> _queue;
        private readonly ConcurrentDictionary<KeyDTO, RedisInstance<KeyDTO, ValueDTO>> _kvps;
        private readonly ILogger _logger;
        private readonly int _maxCapacity;
        private readonly ModuleMetaData _moduleMetaData;
        public readonly int RETRY_INTERVAL = 100;
        private readonly int MaxRetryCount;// default total time 30s 
        private readonly Func<Task> _redisHealingEscalationCB;

        private readonly SemaphoreSlim _semaphore = new(1, 1);
        public RedisWriterOutBox(
            IRedisConnectionManger conn, 
            JsonSerializerOptions jsonOptions, 
            ILogger logger,
            IConfiguration config,
            Func<Task>  escalationCB
            ) : base(conn, jsonOptions)
        {
            _queue = new ConcurrentQueue<KeyDTO>();
            _kvps = new ConcurrentDictionary<KeyDTO, RedisInstance<KeyDTO, ValueDTO>>();
            // This failure point will be report in module event
            _logger = logger.InjectLoggerType(LoggerType.ModuleLog);
            _maxCapacity = config.GetSection("AppMetaData:ModulesMDatas:Redis:MaxRetry").Get<int>();
            _moduleMetaData = config
                .GetSection("AppMetaData:ModulesMDatas:Redis")
                .Get<ModuleMetaData>() ?? new ModuleMetaData { IssuerId = Guid.NewGuid(), IssuerType = Observability.IssuerType.Internal };
            var retryCount = config
                .GetSection("AppMetaData:ModulesMDatas:Redis:MaxRetry")
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
                if (!this.IsConnectionHealthy()) // ensure we know the connection is good
                {
                    throw new RedisConnectionException(ConnectionFailureType.ConnectionDisposed, "Connection Not found");
                }
                if (value is IExtractHashEntries extractable)
                {
                    await this.WriteHashAsync(key, extractable, ttl, ct);
                }
                else await this.WriteAsync(key, value, ttl, ct);
                
            }
            catch (Exception ex)
            {
                using (LogContext.PushProperty("TraceId", new TraceId(
                    issuerId: _moduleMetaData.IssuerId,
                    issuerType: _moduleMetaData.IssuerType)
                ))
                {
                    var moduleName = Assembly.GetEntryAssembly()?.GetName().Name ?? ""; // if missing can leave blank, and opt for traceId
                    _logger.Error($"{moduleName} Redis connection drop, Initiate retry every {RETRY_INTERVAL}ms");
                }
                // Cache stop functioning ashort while only yield minimal performance downgrade <- request db.
                // we wont expect redis connection to drop (else) we (human technician) need to intervent anyway
                
                    await _semaphore.WaitAsync(ct);
                    try
                    {
                        if (Interlocked.Read(ref _queueCount) < _maxCapacity)
                        {
                            var wrappedKvp = new RedisInstance<KeyDTO, ValueDTO> { Key = key, Value = value, TTL = ttl };
                            if (_kvps.TryAdd(key, wrappedKvp))
                            {
                                _queue.Enqueue(key);
                                Interlocked.Increment(ref _queueCount);
                            }
                            else
                            {
                                _kvps[key] = wrappedKvp; // replace, no enqueue
                            }

                        }
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
            }
            // case where some residual in outbox, and we dont have cleanup available
            if (Interlocked.Read(ref _queueCount) == 0 || Interlocked.CompareExchange(ref _isSchedulerRun, 1, 0) != 0)
            {
                return;
            }
            _ = RetryAsync(ct); // Fire and forget safely
        }

        private async Task<bool> TryWriteInternalAsync(CancellationToken ct)
        {
           // this block will be execute under sem lock
            if (this.IsConnectionHealthy() )
            {
                if(!(_queue.TryPeek(out var key) && _kvps.ContainsKey(key)))
                {
                    // we can add low cost thread safe counter to help found an evict policy for poisoned key-value pair
                    _logger.Warning("RedisWriterOutBox:RetryAsync:InternalWrite: Pottential poisoned Key-value pair that at start of queue");
                    return false;
                }
                try
                {
                    if (_kvps[key].Value is IExtractHashEntries extractable)
                    {
                        await this.WriteHashAsync(key, extractable, _kvps[key].TTL, ct);
                    }
                    else await this.WriteAsync(key, _kvps[key].Value, _kvps[key].TTL, ct);
                }
                catch (Exception ex)
                {
                    // Log is still wrapped under context of Retry block
                    _logger.Warning("Issue raise after connection revised");
                    return false;
                }


                // ensure both container are in sync, roll back if not
                _kvps!.Remove(key, out var _);
                _queue.TryDequeue(out _);
                Interlocked.Decrement(ref _queueCount);
                return true;
            }
            return false;
        }

        public async Task RetryAsync(CancellationToken ct = default)
        {
            var count = 0;
            const int HealInterval = 5;
            var moduleName = Assembly.GetEntryAssembly()?.GetName().Name ?? ""; // if missing can leave blank, and opt for traceId
            var operationTraceId = new TraceId(
                    issuerId: _moduleMetaData.IssuerId,
                    issuerType: _moduleMetaData.IssuerType
                );
            // we add the retry number later
            using (LogContext.PushProperty("TraceId", operationTraceId))
            {
                while (Interlocked.Read(ref _queueCount) > 0 && count <= MaxRetryCount)
                {
                    if (count % LogInterval == 0 || count == MaxRetryCount) // avoid pollute log
                        _logger.Warning("{Module}: Attempt retry: {RetryCount}/{MaxRetry}", moduleName, count + 1, MaxRetryCount);

                    await _semaphore.WaitAsync(ct);
                    try
                    {
                        if (!this.IsConnectionHealthy() && count % HealInterval == 0)
                            this.AttemptHeal();

                        await TryWriteInternalAsync(ct);
                        count += 1;
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                    await Task.Delay(RETRY_INTERVAL, ct);
                }
                Interlocked.Exchange(ref _isSchedulerRun, 0);
                if (count > MaxRetryCount)
                {
                    _logger.Warning("{Module}: Redis not recoverable after {MaxRetryCount} attempts. {outBoxCount} items remain unprocessed. Attempt Escalation.",
                        moduleName, count, Interlocked.Read(ref _queueCount));
                    await _redisHealingEscalationCB();
                    return;
                }
                _logger.Information("{Module}: Retry Success: Redis recovered after {Attempts} attempts. Queue flushed.", moduleName, count, _queue.Count);
            }
        }

        public Task<HealthCheckResult> CheckHealthAsync(TraceId traceId, HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (this.IsConnectionHealthy())
            {
                return Task.FromResult(
                    HealthCheckResult.Healthy("Redis is connected", new Dictionary<string, object>
                    {
                        ["OutboxPending"] = Interlocked.Read(ref _queueCount),
                        ["IsSchedulerRunning"] = _isSchedulerRun
                    })
                );
            }

            return Task.FromResult(
                HealthCheckResult.Unhealthy("Redis connection is unavailable")
            );
        }

    }
}
