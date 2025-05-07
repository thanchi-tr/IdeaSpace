using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Context;
using Shared.Infrastructure.Observability;
using Shared.Infrastructure.Redis.Interface.Core;
using Shared.Infrastructure.Redis.Interface.Extension.Recovery;
using Shared.Infrastructure.Redis.Model;
using Shared.Kernel.GeneralConfig;
using Shared.Kernel.Observability.Logging;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Threading;

namespace Shared.Infrastructure.Redis.Core.Write.Extend
{
    /// <summary>
    /// This is an singleton that will live as long as the application.
    /// 
    /// </summary>
    /// <typeparam name="KeyDTO"></typeparam>
    /// <typeparam name="ValueDTO"></typeparam>
    public class RedisWriterOutBox<KeyDTO, ValueDTO> : RedisWriter<KeyDTO, ValueDTO>, IRedisWriteOutBox<KeyDTO, ValueDTO>
        where KeyDTO : IRedisSerialise
    {
        private readonly ConcurrentQueue<RedisInstance<KeyDTO, ValueDTO>> _queue;
        private bool _isSchedulerRun = false;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly ILogger _logger;
        private readonly ModuleMetaData _moduleMetaData;
        public readonly int RETRY_INTERVAL = 100;
        public RedisWriterOutBox(IRedisConnectionManger conn, JsonSerializerOptions jsonOptions, ILogger logger, IConfiguration config) : base(conn, jsonOptions)
        {
            _queue = new ConcurrentQueue<RedisInstance<KeyDTO, ValueDTO>>();
            // This failure point will be report in module event
            _logger = logger.ForContext("Type", LoggerType.ModuleLog);
            _moduleMetaData = config
                .GetSection("AppMetaData:ModulesMDatas:Redis")
                .Get<ModuleMetaData>() ?? new ModuleMetaData { IssuerId = Guid.NewGuid(), IssuerType = Observability.IssuerType.Internal };

        }

        public async Task EnqueueAsync(KeyDTO key, ValueDTO value, TimeSpan ttl, CancellationToken ct = default)
        {
            try
            {
                await _semaphore.WaitAsync(ct);
                try
                {
                    await this.WriteAsync(key, value, ttl, ct);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            catch (RedisConnectionException ex)
            {
                using(LogContext.PushProperty("TraceId", new TraceId
                {
                    IssuerType = _moduleMetaData.IssuerType,
                    IssuerId = _moduleMetaData.IssuerId,
                    Timestamp = DateTime.UtcNow,
                }))
                {
                    _logger.Error($"{Assembly.GetEntryAssembly().GetName().Name} Redis connection drop, Initiate retry every {RETRY_INTERVAL}ms");
                }
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
            // we add the retry number later
            while (_isSchedulerRun || _queue.IsEmpty)
            {
                await _semaphore.WaitAsync(ct);
                try
                {
                    if( _queue.TryDequeue(out var redisInstance))
                        await this.EnqueueAsync(redisInstance.Key, redisInstance.Value, redisInstance.TTL, ct);
                    else
                        _isSchedulerRun = false;
                }
                finally
                {
                    _semaphore.Release();
                }
                await Task.Delay(RETRY_INTERVAL, ct);
            }
        }
    }
}
