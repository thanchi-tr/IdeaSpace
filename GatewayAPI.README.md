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