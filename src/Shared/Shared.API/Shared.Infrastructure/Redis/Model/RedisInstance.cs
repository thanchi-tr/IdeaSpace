using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Infrastructure.Redis.Model
{
    public class RedisInstance<KeyDTO, ValueDTO>
    {
        public KeyDTO Key { get; set; }
        public ValueDTO Value { get; set; }
        public TimeSpan TTL { get; set; }
    }
}
