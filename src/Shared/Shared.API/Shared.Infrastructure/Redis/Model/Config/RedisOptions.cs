using StackExchange.Redis;

namespace Shared.Infrastructure.Redis.Model.Config
{
    public class RedisOptions
    {
        public string ConnectionString { get; set; } = "localhost:6379";
        public bool AbortOnConnectionFail = false;
        public int ConnectRetry = 3;
        public int ConnectTimeout = 5000;
        public int KeepAlive = 180;
        public IReconnectRetryPolicy RetryPolicy { get; set; } = new ExponentialRetry(5000);
    }
}
