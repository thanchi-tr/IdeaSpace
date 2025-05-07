# 📘 Logging Design Overview: Split Logger Strategy

This system uses a **Serilog-powered, context-enriched logging strategy** that provides both modular clarity and full access to Serilog features. It is intentionally **not wrapped in an abstraction** to retain flexibility and control.

## ✅ Why This Design?

- ✔️ Direct access to full `Serilog.ILogger` API (`Information`, `ForContext`, `WriteTo`, etc.)
- ✔️ Minimal abstraction = easier debugging, onboarding, and feature extension
- ✔️ Supports structured logs with enriched context (`Type`, `TraceId`, etc.)
- ✔️ Clean log routing: different types go to different log files

---

## 🧩 How It Works

We inject a single `Serilog.ILogger` and split it into a dictionary by type using `.ForContext("Type", ...)`:

```csharp
// Inside the constructor or service
_logger = baseLogger.Split();
```

Now `_logger` becomes a dictionary of typed loggers:

```
_logger = Dictionary<string, Serilog.ILogger> {
	//...
}_

// where each will have context contain type define in LoggerType
```

Sample Out put:

```
[Module.Event  0f2c4… 12:44:09 INF] IdeaCreated {IdeaId=123, UserId=42}
```

Source: 
> `Shared.Kernel/Observability/Logging/LoggerExtension.cs`

## 🧠 Available Logger Types

> Locate @Shared.Messaging /Observability/Logging
```
public static class LoggerType
{
    public const string ModuleLog = "Module.Event";
    public const string RecoveryLog = "Recovery.Event";
    public const string SystematicLog = "Systematic.Event";
    public const string AuditLog = "Audit";
}

```

These are used to:

Route logs to appropriate files

Filter by purpose (System, Recovery, etc.)

Avoid coupling modules to internal log policies

## 🔍 TraceId Support
You can include scoped trace information (for distributed tracing, observability, or audits): (this is a must for a fully observable system)

> Locate @Shared.Infrastructure /Observability
```
using (LogContext.PushProperty("TraceId", traceId))
{
    _logger[LoggerType.ModuleLog].Information("Event with trace");
}
```
The `TraceId` will automatically appear in:

Console logs

File logs

Any structured JSON sink (e.g. Seq, Elasticsearch, Datadog)

## 📋 Config section
```
// appsettings.json
"Logging": {
  "Path": "Logs",
  "RollingOption": "Day"
}
```

## 🛑 Why Not Abstract with `IStrictLogger`?

While wrapping Serilog with an `IStrictLogger` abstraction is common, we intentionally chose **not to**:

| Reason            | Justification                                         |
|-------------------|-------------------------------------------------------|
| 🔓 **Full control**   | Avoid losing Serilog's fluent API and pipeline       |
| 👨‍💻 **Simplicity**     | No hidden contract; what you call is what you log    |
| 📦 **Internal system** | No need to future-proof for logging vendor swaps     |
| 🧪 **Testability**     | Serilog has its own test sinks if needed             |

---

> 💡 **Abstraction can always be added later if requirements change**, for example:

- When onboarding junior developers
- To standardize logging across multiple tech stacks
- To allow future migration to another logger (e.g., NLog, Elastic)
