# CRUD Application Layer — Resilient Write Pipeline + Trace-Aware Read

This module implements the event-driven core of the `Crud` domain logic. 
It separates reads and writes via a Reader → Repository abstraction, forwarding all writes as self-validating events and enforcing Redis-first reads with SQL fallback.

> Designed for high-observability, fault tolerance, and clean scalability.

## Why This Design?
- ✔️ Immutable Write Events — all mutations go through typed event wrappers (DataModifiedPayload<T>); zero state change in app layer

- ✔️ Redis-first Reader — leverages fast-cache, avoids hitting SQL unless cache is cold

- ✔️ Fallback-Aware — reads fall back to SQL on miss and emit CacheMiss events to patch Redis asynchronously

- ✔️ Traceable by Default — every action logs a TraceId, enriched across logs + published messages

- ✔️ DTO-safe Boundaries — repository never leaks ORM models upward; auto-maps to DTOs with strict shape control


## How It Works
### Repository Responsibilities
- Emits BaseEvent<DataModifiedPayload<T>> on all upserts and deletes

- Logs mutation attempts and exceptions using ModuleLog

- Accepts only DTO values (never ORM), enforcing DTO contract boundaries

- No direct state persistence — all mutations are async and delegated to event consumers

```
await _publisher.PublishAsync(
    new BaseEvent<DataModifiedPayload<DTOType>>
    {
        CorrelationId = traceId.ToString(),
        Payload = new DataModifiedPayload<DTOType>
        {
            Type = ModificationEventType.Delete,
            Data = key
        }
    });

```

###  Reader Responsibilities
- Checks Redis first (ReadAsync(key)), emits CacheHit if found

- If not in Redis, queries SQL (FindAsync(key)), emits CacheMiss

- Never crashes on miss or fallback — logs cleanly, always trace-scoped

```
await _publisher.PublishAsync(new BaseEvent<CacheEventPayload<ORMType>>(
    correlationId: traceId.ToString(),
    payload: new CacheEventPayload<ORMType>
    {
        Type = CacheEventType.CacheMiss,
        Data = resultFromDb
    }));

```

### Event Models
`DataModifiedPayload<T>`
| Field  | Type                    | Notes                                             |
| ------ | ----------------------- | ------------------------------------------------- |
| `Type` | `ModificationEventType` | `Upsert`, `Delete`, `SoftDelete` (future-proofed) |
| `Data` | `T`                     | The DTO object itself (always fully typed)        |


`CacheEventPayload<T>`
| Field  | Type             | Notes                             |
| ------ | ---------------- | --------------------------------- |
| `Type` | `CacheEventType` | `CacheHit`, `CacheMiss`           |
| `Data` | `T`              | The ORM model used to patch Redis |

##  Key DTO Contract

All DTOs implement:
```
public interface IHasKey<TKey> { TKey GetKey(); }

public interface IRedisSerialise
{
    RedisKey ToRedisKey();
}

```
## Message Topology

- Exchange: `sys.operate.lazy` - asynchronous event pool where eventually request will be parse and result send but no contract on when it is complete
- Queue:
	- `cache.operate.lazy.queue` - for sending a manual tracked entity (issue by SQL data stakeholder) to cache it.
	- DLQ: TTL - 2s (shortlive) cleanup occur after retry 5 (3 time in total) <= we want to detect failure with cache asap
	- `modify.operate.lazy.queue` - for Crud to notify gatekeeper that a resource is changed
	- DLQ: TTL - 30s (shortlive) cleanup occur after retry 5 (5 time in total) <= asynchronous action: give time for it to be processes
	- `create.operate.lazy.clean` - for Crud to notify gatekeeper that a resource is created
	- DLQ: TTL - 30s (shortlive) cleanup occur after retry 5 (5 time in total) <= asynchronous action: give time for it to be processes

	## Initialization Rules

- Health check queue is ephemeral, TTL 30s
- Reinitialization occurs if health fails for >3s