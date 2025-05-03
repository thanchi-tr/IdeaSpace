# Client Notification Module

## Message Topology

- Exchange: `request.result`
- Queue:
	- `modify.request.result.queue` - for sending the status of the request (to modify)
	- `get.request.result.queue` - for sending the status of the request (to create)
	- `clean.request.result.clean` - for clearing mediator(manually) command


	## Initialization Rules

- Health check queue is ephemeral, TTL 30s
- Reinitialization occurs if health fails for >3s