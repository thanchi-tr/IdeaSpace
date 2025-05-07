using Shared.Infrastructure.Redis.Interface.Core;
using Shared.Infrastructure.Redis.Interface.Core.Redis;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Shared.Infrastructure.Redis.Core.Write
{
    public class RedisWriter<KeyDTO, ValueDTO> : IWrite<KeyDTO, ValueDTO>
        where KeyDTO: IRedisSerialise
    {
        private readonly IDatabase _db;
        JsonSerializerOptions _jsonOptions;

        public RedisWriter(IRedisConnectionManger conn, JsonSerializerOptions jsonOptions)
        {
            _db = conn.GetDatabase(-1);
            _jsonOptions = jsonOptions;
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
    }
}
