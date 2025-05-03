# API GATEWAY

## Role:

- Act as a service finder.
- Auth guard.
- Act as rate limiter per user (if still have available bucket)
- Forward request to desired service
- As well as register client mediator (a path way/ webhook 
[similiar to a CB but inter system + multi protoco communication] that allow system to serve the update later)
- Act as the single source of entry to our system.
- Only request issue by API gateway can communicate with other service

## Message Topology

- null

## Initialization Rules

- Health check queue is ephemeral, TTL 30s
- Reinitialization occurs if health fails for >3s