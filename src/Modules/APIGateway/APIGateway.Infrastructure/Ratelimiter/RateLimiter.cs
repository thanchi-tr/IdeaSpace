using APIGateway.Infrastructure.Interface.Ratelimiter;
using APIGateway.Infrastructure.Model.DTO;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Context;
using Shared.Infrastructure.Constant;
using Shared.Infrastructure.Observability;
using Shared.Infrastructure.Redis.Config;
using Shared.Infrastructure.Redis.Interface.Core.Redis;
using Shared.Infrastructure.Redis.Model.DTO.Key;
using Shared.Kernel.Observability.Logging.Constant;

namespace APIGateway.Infrastructure.Ratelimiter
{
    public class RateLimiter : IRateLimit
    {
        private readonly IRead<RedisGatewayRateLimitBucketKey, RediRateBucket> _reader;
        private readonly IWrite<RedisGatewayRateLimitBucketKey, RediRateBucket> _writer;
        // we only logging request of extract bucket in order to detect malbahaviour
        private readonly ILogger _auditLog;
        private IConfiguration _configuration;
        private readonly Dictionary<Guid, int> _refileRates = new Dictionary<Guid, int>();
        private readonly int DefaultResetMin;
        public RateLimiter(
            IConfiguration configuration,
            IRead<RedisGatewayRateLimitBucketKey,
            RediRateBucket> reader,
            IWrite<RedisGatewayRateLimitBucketKey, RediRateBucket> writer,
            ILogger auditLog)
        {
            _reader = reader;
            _writer = writer;
            _auditLog = auditLog.ForContext("Type", LoggerType.AuditLog);
            _configuration = configuration;

            // extract module
            var meta = configuration.GetSection("AppMetaData").Get<AppMetaData>();
            foreach (var moduleMetaData in meta.ModulesMDatas.Keys)
            {
                _refileRates.Add(meta.ModulesMDatas[moduleMetaData].IssuerId, meta.ModulesMDatas[moduleMetaData].RefillRate);
            }
            DefaultResetMin = meta.ModulesMDatas[AvailableService.RateLimiter].Ttl;
        }

        public async Task<bool> IsAllowRequestAsync(string userId, Guid moduleId)
        {
            var traceId = _configuration.ExtractTraceId(AvailableService.RateLimiter);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var validUserId))
            {
                using (LogContext.PushProperty("TraceId", traceId))
                    _auditLog.Warning("Received a stale or null request without userId.");
                return false;
            }

            var key = new RedisGatewayRateLimitBucketKey
            {
                UserId = validUserId,
                ModuleId = moduleId
            };

            var bucketRedisEntries = await _reader.ReadHashAsync(key);
            var bucket = new RediRateBucket(bucketRedisEntries!);
            if (bucket == null || bucketRedisEntries.Length == 0)
            {
                // first time access
                bucket = new RediRateBucket(remainCount: _refileRates[moduleId] - 1, maxCapacity: _refileRates[moduleId]);
            }

            else
            {
                // Refill logic: 
                var timeGap = DateTime.Now.Ticks - bucket.LastUpdatedEpoch;
                bucket.LastUpdatedEpoch = DateTime.Now.Ticks;
                bucket.RemainCount += (int)(timeGap / (long)_refileRates[moduleId]);
                bucket.RemainCount = bucket.RemainCount > bucket.MaxCapacity ? bucket.MaxCapacity : bucket.RemainCount;
                if (bucket.RemainCount <= 0)
                {
                    using (LogContext.PushProperty("TraceId", traceId))
                        _auditLog.Warning("Rate limit exceeded for user {UserId} in module {ModuleId}", userId, moduleId);
                    return false;
                }
                bucket.RemainCount--;
            }
            await _writer.WriteAsync(key, bucket, TimeSpan.FromMinutes(DefaultResetMin)); // TTL can be configurable, after 30 minute, then their rate reset
            using (LogContext.PushProperty("TraceId", traceId))
                _auditLog.Verbose("Module Access: {ModuleId} grant to {UserId}", moduleId, userId);
            return true;
        }

    }
}
