# Crud Module

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