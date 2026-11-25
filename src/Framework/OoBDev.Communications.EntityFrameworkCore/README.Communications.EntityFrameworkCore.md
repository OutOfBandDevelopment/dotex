# OoBDev.Communications.EntityFrameworkCore

Durable stores for the communications layer in a relational database: an `ISuppressionList` (bounces, complaints and unsubscribes survive restarts and are shared by every instance) and the orchestrator stores `IDeferralStore` and `ITargetPreferenceProvider`.

| Type | Role |
|------|------|
| `OrchestrationDbContext` / `DeferredRequestRow` / `TargetPreferenceRow` | The `DeferredRequests` table (indexed by due time, data as JSON) and the `TargetPreferences` table keyed by target id. |
| `EfDeferralStore` | `IDeferralStore`: held requests survive restarts; `TakeDueAsync` deletes each row by id and keeps only the rows this call deleted, so two instances never release the same request. Data comes back as plain values (string, long, double, bool, null). |
| `EfTargetPreferenceProvider` | `ITargetPreferenceProvider` plus `SetAsync` to store or replace a target's channels, culture, time zone, minimum priority and quiet hours. |

| Type | Role |
|------|------|
| `SuppressionsDbContext` / `SuppressionRow` | The `Suppressions` table keyed by the lower-cased address, with reason and added time. |
| `EfSuppressionList` | `ISuppressionList`: add (the first reason stays when two writers race), remove, check. Addresses compare case-insensitively. |

```csharp
services.TryAddEntityFrameworkSuppressionList((sp, o) => o.UseSqlite("Data Source=suppress.db"));
services.AddEmailSuppression(); // wraps ISendService; uses this list instead of the in-memory one
services.AddUnsubscribeLinks();  // OoBDev.Communications: signed RFC 8058 links feeding the same list
```

The table can live in a schema or behind a prefix: `o.UseStoreInstance(schema: "mail")` ([OoBDev.Data.EntityFrameworkCore](../OoBDev.Data.EntityFrameworkCore/README.Data.EntityFrameworkCore.md)). Register it before `AddEmailSuppression`. The consumer picks the provider and creates the schema (`EnsureCreated` or its own migrations; shipped migrations are not built).

Tests: `OoBDev.Communications.EntityFrameworkCore.Tests` (Simulate, Sqlite in memory). Not run: SQL Server and PostgreSQL.

```csharp
services.TryAddEntityFrameworkOrchestration((sp, o) => o.UseSqlite("Data Source=comms.db"));
services.TryAddCommunicationsHosting(); // orchestrator and release loop use these stores instead of the in-memory ones
```

Register the stores before the orchestrator. Tests: `EfOrchestrationStoreTests` (Simulate, Sqlite in memory).
