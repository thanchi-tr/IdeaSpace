IdeaSpace
My Learning As A Service (LAAS) Modular Monolith that lean toward micro service. Where it is a complete distributed system( using Rabbit MQ).

About
This project implements a high-resilience, event-sourced modular monolith architecture.

It features:
• Event-Driven Core: All domain changes are captured as immutable DeltaLog events published via RabbitMQ.

• Batch-Based Processing: Client operations are staged and flushed atomically in optimized batches, reducing DB load.

• Bulk Persistence: Entity operations are staged and committed efficiently using EFCore BulkExtensions.

• Distributed Stakeholders: SQL persistence, Redis caching, and Recovery orchestration operate independently via decoupled event consumption.

• Soft Recovery (Live Repair): Real-time sequence tracking auto-heals missing events using Delta replay.

• Hard Recovery (Crash Restore): Full snapshot + delta replay restores system state after catastrophic failures.

• Observability: Full TraceId propagation for end-to-end distributed tracing across all modules.

• Scalability Ready: Stakeholders are modular and horizontally scalable. Recovery designed for large data volumes.

Architecture Overview
• 🛠 Modular Monolith (feature-isolated, clean layering)

• 📦 RabbitMQ Event Mesh (Topic exchanges for decoupled communication)

• 🗄 EFCore + BulkExtensions (optimized batch writes)

• 🧠 In-Memory State Tracking + Snapshot Serialization

• 💾 Redis Caching for fast reads

• 🔄 Crash-Tolerant Recovery Pipeline

• 🕵️ Full Event Audit Trail with TraceIds

• 🛡 Dead Letter Queues for fault isolation

Design Diagram:
Store under docs/\*
