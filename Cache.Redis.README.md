# 📘 Redis Infrastructure Overview: Durable + Guarded Local Setup

This repository’s Redis layer provides a **developer-friendly, failure-tolerant cache** that mirrors production expectations (persistence, auth, memory guard) while remaining one-command (`docker compose up`) for local use.

---

## ✅ Why This Design?

- ✔️ **AOF persistence** — survives laptop crashes; no silent data loss during dev sessions  
- ✔️ **AUTH on by default** — matches prod security habits; avoids hard-coding “no-password” in code  
- ✔️ **Memory & eviction guard** — caps cache at 256 MB so runaway TTL keys can’t OOM your machine  
- ✔️ **Health-checks & reconnect policy** — services self-heal without crashing on brief Redis blips  
- ✔️ **Single compose service** — zero extra tooling; still upgrade-ready to Sentinel/Cluster later  

---

## 🧩 How It Works

We spin up Redis with explicit runtime flags and map a named Docker volume for AOF / RDB files.

```yaml
# /docker-compose.yml excerpt
redis:
  image: redis:latest
  command: >
    redis-server
      --appendonly yes
      --appendfsync everysec
      --requirepass devpass
      --maxmemory 256mb
      --maxmemory-policy volatile-ttl
  volumes:
    - redis_data:/data
  healthcheck:
    test: ["CMD", "redis-cli", "-a", "devpass", "PING"]
```
Inside .NET code we bind those values via `RedisOptions`

```
// appsettings.json
"Redis": {
  "ConnectionString": "localhost:6379",
  "Password": "devpass",
  "ConnectRetry": 5,
  "RetryPolicy": "ExponentialRetry(5000)"
}
```
Then we can parse this into 
(using `RedisOptions` @Shared/Infrastructure/Redis/Model/Config)
```
var cfg = ConfigurationOptions.Parse<RedisOptions>(opts.ConnectionString);
cfg.Password              = opts.Password;
cfg.ConnectRetry          = opts.ConnectRetry;
cfg.ReconnectRetryPolicy  = new ExponentialRetry(5000);
```
## 🧰 RedisWriterOutBox

This write-through wrapper buffers writes to Redis in case of connection drop (i.e., Redis is down but the app is still running). It retries every 100ms until Redis is reachable again.

- Respects TTL : NOTE: at base class, we ensure infrastructure failure on fail to pass, set up using ttl to
    to eliminate silence break point
- Is thread-safe via `SemaphoreSlim`
- Uses a `ConcurrentQueue` as the fallback store
- Logs failure using `ModuleLog` type with full `TraceId` context

### 👇 Auto-Routed Write Behavior
| Value Type                          | Redis Write Strategy |
|------------------------------------|-----------------------|
| Implements `IExtractHashEntries`   | `HashSetAsync(...)`   |
| Any other DTO                      | `StringSetAsync(...)` |

This logic is embedded in `EnqueueAsync()` so that upstream services don’t need to worry about Redis specifics.

### Sample TraceId Log
## 🔑 Key DTO → Redis Key Conventions
Every cache key comes from a DTO that implements `IRedisSerialise`.

```
public record GateCacheKey(Guid traceId, Guid batchId, Guid ideaId)
        : IRedisSerialise
{
    public RedisKey ToRedisKey() =>
        $"GateKeeper:Idea:{traceId}:{batchId}:{ideaId}";
}
```
| Module           | Key Prefix     | TTL                      |
| ---------------- | -------------- | ------------------------ |
| **GateKeeper**   | `GateKeeper:*` | 90 min (batch window)    |
| **Notification** | `Mediator:*`   | 5 min (client handshake) |
| **RateLimiter**  | `Rate:User:*`  | 60 sec (token-bucket)    |

(See `Shared.Infrastructure.Redis.Model` for DTO definitions.)

---

## 🛡️ Resilience Features
| Feature              | Where it’s enabled                                                  | Details                                            |
| -------------------- | ------------------------------------------------------------------- | -------------------------------------------------- |
| **AOF journaling**   | Compose flag `--appendonly yes`                                     | Flushes to disk every 1 s.                         |
| **Auth**             | `--requirepass devpass` + `cfg.Password`                            | Prevents accidental no-auth in prod code.          |
| **Retry / back-off** | `cfg.ConnectRetry=5`, `ReconnectRetryPolicy=ExponentialRetry(5000)` | Logs but doesn’t crash services during hiccups.    |
| **Memory cap**       | `--maxmemory 256mb --maxmemory-policy volatile-ttl`                 | Evicts only TTL keys if the cap is hit.            |
| **Health-check**     | Compose probe + .NET `RedisHealthCheck`                             | `GET /health` returns `Healthy` only after `PING`. |
| **MaxRetry + escaltion cb**     | inject context + escalation hook                             | if retry fail more than a config max retry, system will call the escalation hook |

---
## 🏃‍♂️ Local Setup & Commands

```
# create shared network once
docker network create ideaspace_network

# spin Redis up
docker compose up -d redis

# connect
redis-cli -a devpass
> PING
PONG
```

Logs live under the `redis_data/` volume:

```
docker volume inspect redis_data
```

---
## 🔭 Future Upgrade Path
| Stage                 | How to move forward                                                                  |
| --------------------- | ------------------------------------------------------------------------------------ |
| **High Availability** | Convert to a 3-node Sentinel cluster; pass sentinel endpoints in `ConnectionString`. |
| **Sharding**          | Upgrade to Redis Cluster (6 nodes) if > 10 GB RAM or > 100 k ops/s.                  |
| **Metrics**           | Add Grafana/Redis-Exporter; alert on `evicted_keys`, `used_memory_ratio`.            |
| **Backups**           | Nightly copy of `appendonly.aof` / RDB to S3.                                        |
| **ACL roles**         | Create `cacheReader`, `rateLimiter` users; rotate keys via Vault.                    |
