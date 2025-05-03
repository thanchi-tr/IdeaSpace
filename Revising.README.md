# Revising Module

## Message Topology

- Exchange: `sys.operate.eager` - time sensitive action, that expect result (get validate) to be resolve asap
- Queue:
	- `validate.operate.eager.queue` - for sending the request for validate a revised answer attempt.
	- DLQ: TTL - 3s (shortlive) cleanup occur after retry twice (3 time in total)


	## Initialization Rules

- Health check queue is ephemeral, TTL 30s
- Reinitialization occurs if health fails for >3s