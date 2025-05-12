namespace Shared.Infrastructure.Redis.Config
{
    public class CatastrophicHandlerConfig
    {
        public int RetryInterval { get; set; }
        public int MaxRetryCount { get; set; }
        public int SelfHealInterval { get; set; }
    }
}
