using Shared.Infrastructure.Observability;

namespace Shared.Infrastructure.Redis.Config
{
    public class RedisConfig
    {
        public IssuerType IssuerType { get; set; }
        public Guid IssuerId { get; set; }
        public int OutBoxCapacity { get; set; }
    }
}
