using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Infrastructure.Redis
{
    public class RedisOptions
    {
        public string ConnectionString { get; set; } = "localhost:6379";
        public bool AbortOnConnectionFail = false;
        public int ConnectRetry = 3;
        public int ConnectTimeout = 5000;
        public int KeepAlive = 180;
    }
}
