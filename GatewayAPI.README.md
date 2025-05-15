# API GATEWAY

## Role:

- Service Discovery: Acts as a smart router for downstream services.
- Authentication & Trace Guard:
	- Validates JWT tokens using the JwtValidator, with Redis-based white/blacklist caching.
	- Injects TraceId to all logs and messages for distributed tracing.
- Act as rate limiter per user (if still have available bucket)
- Forward request to desired service
- As well as register client mediator (a path way/ webhook 
[similiar to a CB but inter system + multi protoco communication] that allow system to serve the update later)
- Act as the single source of entry to our system.
- Only request issue by API gateway can communicate with other service

## Message Topology

- Currently supports **RESTful API** (HTTP).
- Future roadmap includes **SignalR / WebSocket** support for bi-directional real-time messaging.


## Initialization Rules

| Resource              | Rule                                                        |
| --------------------- | ----------------------------------------------------------- |
| 🩺 Health Check Queue | Ephemeral queue, TTL: 30 seconds, non-durable               |
| ❗ Reinitialization    | Triggered if health checks fail for more than **3 seconds** |
| 🔑 JWT Validation     | Performed using `JwtValidator` with:                        |

## 🔐 JwtValidator Integration Details


- Whitelist Redis:
    - Stores recently validated JWTs with UserContext (match ttl);
    - Keyed by RedisJWTKey(JWT)
- Blacklist Redis:
    - Infrastructure in place for access token revolk.
- Trace:
    - Inject with Type: Client and Id: UserID 
    - @Todo: modified the TraceId so that it include IP address (helpful interm of internal module too)

## JWT Replay Protection
To align with `JWT security best practices`, our system now leverages the `jti` (JWT ID) claim — a unique identifier for each issued token — to prevent replay attacks.

###  How It Works
| Aspect              | Design Detail                                                                                                                               |
| ------------------- | ------------------------------------------------------------------------------------------------------------------------------------------- |
| 🔑 **jti** Claim    | Required in every token issued by the Auth provider (e.g., Auth0, IdentityServer).                                                          |
| 🧠 **Whitelist**    | Upon first successful validation, the token’s `jti` is cached in Redis (UserWhiteList) with TTL aligned to token expiry.                    |
| 🚫 **Blacklist**    | If a token is manually revoked or expires naturally, its `jti` is added to the Redis blacklist (`HardDeny`) to prevent reuse.               |
| 🧪 **Replay Guard** | All subsequent uses of the same token are checked against the whitelist and blacklist — ensuring no replay succeeds after logout or expiry. |


### Why This Matters


Replay attacks occur when an attacker captures a valid JWT and reuses it to impersonate a user. By enforcing jti caching and tracking:

Tokens are one-time-use from the system’s perspective.

Tokens revoked early (e.g., logout) become immediately invalid.

Observability is enhanced, and abuse is traceable via audit logs.

### Key Implementation Files


- `JwtValidator.cs` : core logic enforcing whitelist/blacklist with jti.
- `RedisWLJWTKey` / `RedisBLJWTKey`: strongly typed Redis keys aligned with our system prefixing strategy.
- 
## Example Log


1. Initial Log in
```
{
  "level": "Information",
  "message": "User xuan.trinh login successful at 2025-05-08T10:01:12Z",
  "TraceId": {
    "IssuerType": "Client",
    "IssuerId": "d15b9f70-8735-482e-997e-3fbb693e9db0",
    "Timestamp": "2025-05-08T10:01:12Z"
  }
}
```
2. Whitelist hit
```
{
  "level": "Information",
  "message": "User xuan.trinh relogin successful at 2025-05-08T10:01:12Z",
  "TraceId": {
    "IssuerType": "Client",
    "IssuerId": "d15b9f70-8735-482e-997e-3fbb693e9db0",
    "Timestamp": "2025-05-08T10:01:12Z"
  }
}
```
3. Blacklisted Token Usage
```
{
  "level": "Warning",
  "message": "User login rejected at 2025-05-08T10:02:47Z using Blacklisted Token.",
  "TraceId": {
    "IssuerType": "Client",
    "IssuerId": "d15b9f70-8735-482e-997e-3fbb693e9db0",
    "Timestamp": "2025-05-08T10:02:47Z"
  }
}
```

4. Jwt Validate Failure
```
{
  "level": "Warning",
  "message": "JWT validation failed",
  "exception": {
    "type": "Microsoft.IdentityModel.Tokens.SecurityTokenInvalidSignatureException",
    "message": "IDX10503: Signature validation failed.",
    "stackTrace": "..."
  },
  "TraceId": {
    "IssuerType": "Client",
    "IssuerId": "d15b9f70-8735-482e-997e-3fbb693e9db0",
    "Timestamp": "2025-05-08T10:03:30Z"
  }
}
```

## 🚦 Rate Limiter Integration
To enforce `per-user, per-module` rate limits, we implemented a token-bucket rate limiter backed by Redis.

### How It Works

| Aspect              | Design Detail                                                                                   |
| ------------------- | ----------------------------------------------------------------------------------------------- |
| 🪣 **Token Bucket** | Each user-module pair has a dedicated bucket (`RemainCount`, `LastUpdatedEpoch`, `MaxCapacity`) |
| ⚙️ **Lazy Refill**  | Token refill calculated based on elapsed time since last access — no background job needed      |
| ⏳ **TTL Reset**     | Buckets expire after `Ttl` (e.g. 30 mins); new ones are created on demand                       |
| 📦 **Redis Hash**   | Bucket fields stored as Redis Hash (`HashSetAsync`, `HashGetAllAsync`)                          |

### Configuration

Rate limiter parameters are defined in `AppMetaData.ModulesMDatas:`

```
"RateLimiter": {
  "IssuerId": "6c2f78b9-9821-48f9-bd08-c0865a602a35",
  "IssuerType": "0",
  "RefillRate": 3,
  "Ttl": 30
}
```

### Sample DTO
```
public class RediRateBucket : IExtractHashEntries
{
    public int RemainCount { get; set; }
    public long LastUpdatedEpoch { get; set; }
    public int MaxCapacity { get; set; }

    // Hash extraction logic omitted for brevity
}

```

### Key Implementation Files
- `RateLimiter.cs` – Main logic using Reader/Writer Redis abstraction.

- `APIGateway.Infrastructure/Model/DTO/RediRateBucket.cs` – Bucket data model with serialization helpers.

- `APIGateway.Infrastructure/Model/DTO/RedisGatewayRateLimitBucketKey.cs` – Redis key contract for rate limiter.

- `Share.Infrastructure/Redis/Config/AppMetaData.cs` – Container: Config-driven module rate TTL + refill logic.

- `Share.Infrastructure/Redis/Config/ModuleMetaData.cs` – Config-driven module rate TTL + refill logic.
### Example Log (Audit log only)

#### Access granted:
```
{
  "level": "Verbose",
  "message": "Module Access: Crud grant to 123e4567-e89b-12d3-a456-426614174000",
  "TraceId": {
    "IssuerType": "RateLimiter",
    "IssuerId": "6c2f78b9-9821-48f9-bd08-c0865a602a35",
    "Timestamp": "2025-05-08T10:03:30Z"
  }
}
```

#### Rate Limit Exceeded
```
{
  "level": "Warning",
  "message": "Rate limit exceeded for user 123e4567-e89b-12d3-a456-426614174000 in module Crud",
  "TraceId": {
    "IssuerType": "RateLimiter",
    "IssuerId": "6c2f78b9-9821-48f9-bd08-c0865a602a35",
    "Timestamp": "2025-05-08T10:03:35Z"
  }
}
```