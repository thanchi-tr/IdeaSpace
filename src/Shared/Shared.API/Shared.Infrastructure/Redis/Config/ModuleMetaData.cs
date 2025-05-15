using Shared.Infrastructure.Observability;
namespace Shared.Infrastructure.Redis.Config
{
    public class ModuleMetaData
    {
        public IssuerType IssuerType { get; set; }
        public Guid IssuerId { get; set; }
        public int RefillRate { get; set; }
        public int Ttl {  get; set; }
        public string Url { get; set; }
        public int RetryCount { get; set; }
        public int HttpClientTimeOut { get; set; }
    }
}
