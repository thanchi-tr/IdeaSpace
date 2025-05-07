using Shared.Infrastructure.Redis.Interface.Core;
using Shared.Infrastructure.Redis.Interface.Core.Redis;
using StackExchange.Redis;
using System.Text.Json;

namespace Shared.Infrastructure.Redis.Core.Read
{
    /// <summary>
    /// Separation of Reader (CQRS alignment)
    ///     - allow scalling(config reader differ from writer)
    /// </summary>
    /// <typeparam name="KeyDTO"></typeparam>
    /// <typeparam name="ValueDTO"></typeparam>
    public class RedisReader<KeyDTO, ValueDTO> : IRead<KeyDTO, ValueDTO>
        where KeyDTO : IRedisSerialise
    {
        private readonly IDatabase _db;
        JsonSerializerOptions _jsonOptions;

        public RedisReader(IRedisConnectionManger conn, JsonSerializerOptions jsonOptions)
        {
            _db = conn.GetDatabase(-1);
            _jsonOptions = jsonOptions;
        }


        public async Task<ValueDTO?> ReadAsync(KeyDTO key, CancellationToken ct = default)
        {
            var redisKey = key.ToRedisKey();
            // Attempt to read the value, start with master, if not exist going to replica
            var redisRes = await _db.StringGetAsync(redisKey, CommandFlags.PreferMaster);

            return redisRes.HasValue
                    ? JsonSerializer.Deserialize<ValueDTO>(redisRes.ToString(), options: _jsonOptions)
                    : default;
        }
    }
}
