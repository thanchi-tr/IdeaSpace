# 💡 IdeaSpace: Learning as a Service Platform
**IdeaSpace** is a high-resilience, modular monolith with microservice-leaning architecture. It delivers event-driven coordination, fault-tolerant recovery, and fully observable system behavior — built for long-term maintainability and scale.

## Status: Complete phase 2 : Setting up Infrastructure.
---
## 🧠 Core Philosophy

This project adopts a **distributed stakeholder model**:
- Modular services **coordinate via events**, not direct calls.
- All state changes are **captured, auditable, and recoverable**.
- Modules can be **independently deployed or scaled** as needed.

---

## ⚙️ Key Features

| Category                | Description                                                                 |
|-------------------------|-----------------------------------------------------------------------------|
| 🧩 Modular Monolith     | Feature-isolated folders with layered separation (.API / .Worker / .Domain) |
| 📦 RabbitMQ Backbone    | Topic-based routing and fan-out messaging                                   |
| 📑 DeltaLog Pattern     | All domain changes are event-sourced and published                          |
| 💽 EFCore Bulk Write    | Batch persistence via `EFCore.BulkExtensions`                               |
| 🔁 Retry & Recovery     | Soft (live fix) and hard (delta replay) recovery modes                      |
| 🚦 Dead Letter Queues   | Fault isolation via structured retry channels                               |
| 🧵 TraceId Propagation  | Every operation traceable across module boundaries                          |
| 🚀 Redis Integration    | High-speed caching with TTL + Outbox retry                                  |
| 📊 Observability        | Serilog logs + categorized file sinks + trace context                       |

---
## 🗺 Architecture Overview
[Client] → [API Gateway] → [RabbitMQ Exchange] → [Crud | GateKeeper | ExpirationDisplay | Revising | Notification | Recovery]
→ [Redis Cache (Outbox, WL/BL)]  → [SQL DB (batch persisted)]


## Starting Guide


> ⚠️ Each module listens for specific events and acts independently based on contract and responsibility.

---

## 🏗 Initialization Requirements

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Docker & Docker Compose

### 1️⃣ Restore & Build

```bash
dotnet restore
```

### 2️⃣ Start Infrastructure

```
docker-compose -f docker-compose.yml up -d
```

### 3️⃣ Start Application Modules
```
docker-compose -f docker-compose.module.yml up -d
```

## Port mapping:(dev)

| Service               | Port  | Type         |
| --------------------- | ----- | ------------ |
| 🧩 API Gateway        | 5004  | External API |
| 🧠 Expiration Display | 5101  | Internal API |
| 🔒 Data GateKeeper    | 5102  | Internal API |
| 📋 CRUD               | 5103  | Internal API |
| 📣 Notification       | 5105  | Internal API |
| 🐘 PostgreSQL         | 5432  | DB           |
| 🧠 Redis              | 6379  | Cache        |
| 📬 RabbitMQ           | 5672  | Messaging    |


# 📂 Documentation
- System design (architecture, event flows, responsibilities): docs/

- Logging Strategy: docs/Logging.md

- API Gateway Auth & JWT Flow: docs/APIGateway.md

>💡 Main system design created using [draw.io], export as .drawio.svg in /docs.