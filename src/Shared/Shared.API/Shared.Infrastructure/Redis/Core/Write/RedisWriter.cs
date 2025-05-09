using Shared.Infrastructure.Redis.Interface.Core;
using Shared.Infrastructure.Redis.Interface.Core.Redis;
using Shared.Infrastructure.Redis.Interface.Extension.Operation;
using StackExchange.Redis;
using System.Text.Json;

namespace Shared.Infrastructure.Redis.Core.Write
{
    /// <summary>
    /// NOTE: Explicitly not provide default value for any of the config (ttl, key)
    ///     pitfall: with default value, weak system setting, fail environment (production) variable read
    ///         is forgive, and likely be hidden among start up message.
    ///         This lead to undesired behavior.
    /// NOTE: This class provide base contract without handling exception.
    /// NOTE: This lack of exception handling (redis connection drop)
    ///     need explicitly handling those to ensure production capability
    /// </summary>
    /// <typeparam name="KeyDTO"></typeparam>
    /// <typeparam name="ValueDTO"></typeparam>
    public abstract class RedisWriter<KeyDTO, ValueDTO> : IWrite<KeyDTO, ValueDTO>
        where KeyDTO: IRedisSerialise
    {
        private IDatabase _db {get; set;}
        private  IRedisConnectionManger _conn { get; set; }
        JsonSerializerOptions _jsonOptions;

        public RedisWriter(IRedisConnectionManger conn, JsonSerializerOptions jsonOptions)
        {
            _conn = conn;
            _db = conn.GetDatabase(-1);
            _jsonOptions = jsonOptions;
        }

        public virtual void AttemptHeal()
        {
            _conn.AttemptHeal();
            _db = _conn.GetDatabase(-1);
        }

        public virtual bool IsConnectionHealthy()
        {
            return _conn.IsConnectionHealthy();
        }

        /// <summary>
        /// Write Async have the promise to complete write and 
        /// will override existed value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="ttl"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task WriteAsync(KeyDTO key, ValueDTO value, TimeSpan ttl, CancellationToken ct = default)
        {
            var serialisedValue = JsonSerializer.Serialize(value);
            var redisKey = key.ToRedisKey();
            await _db.StringSetAsync(redisKey, serialisedValue, ttl, When.Always);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="ttl"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task WriteHashAsync(KeyDTO key, IExtractHashEntries value, TimeSpan ttl, CancellationToken ct = default)
        {
            var redisKey = key.ToRedisKey();
            var hashEntries = value.ToHashEntries();
            await _db.HashSetAsync(redisKey, hashEntries, CommandFlags.PreferMaster);
            await _db.KeyExpireAsync(redisKey, ttl);
        }
    }
}
