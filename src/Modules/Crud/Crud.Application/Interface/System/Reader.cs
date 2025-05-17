using Shared.Infrastructure.Redis.Core.Read;
using Shared.Infrastructure.Redis.Interface.Core;

namespace Crud.Application.Interface.System
{
    public class Reader<KeyDTO, ValueDTO> 
        where KeyDTO : IRedisSerialise
    {
        private RedisReader<KeyDTO, ValueDTO> _cache { get; set; }

    }
}
