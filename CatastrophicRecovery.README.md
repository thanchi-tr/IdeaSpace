# Catastrophic Recovery Module

## Message Topology

- Exchange: `exchange.recover`
- Queue:
	- `sys.recover.delta.queue` - for passing Delta Log event from gate keeper's data stake holder to recover module
	- `sys.recover.command.queue` - for issueing recovery command (soft, hard, review)
	- `sys.recover.clean.queue` - for flush out expired in memory delta log, or synchronise batch id


	## Initialization Rules

- Health check queue is ephemeral, TTL 30s
- Reinitialization occurs if health fails for >3s