# Expiration Display Module

## Message Topology

- Exchange: `sys.operate.eager` - time sensitive action, that expect result (get action) to be resolve asap
- Queue:
	- `view.operate.eager.queue` - for sending the request for get resources.
	- DLQ: TTL - 5s (shortlive) cleanup occur after retry twice (2 time in total)



	## Initialization Rules

- Health check queue is ephemeral, TTL 30s
- Reinitialization occurs if health fails for >3s