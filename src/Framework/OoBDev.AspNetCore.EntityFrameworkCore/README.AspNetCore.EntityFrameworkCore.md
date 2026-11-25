# OoBDev.AspNetCore.EntityFrameworkCore

A durable audit trail of application right checks, with retention. `AddRightAuditLog()` (in `OoBDev.AspNetCore.Mvc`) already logs every check; this package also stores each one so an auditor can ask who was denied what, and when.

| Type | Role |
|------|------|
| `IRightAuditSink` / `RightAuditEntry` (in `OoBDev.AspNetCore.Abstractions`) | The contract `AuditingApplicationRightChecker` writes to. `Record` never blocks or throws. |
| `BufferedRightAuditSink` | A bounded buffer (`QueueCapacity`). When it is full the entry is dropped and counted in `Dropped`: an audit write must not stop a request. |
| `RightAuditWriter` | Hosted service: flushes the buffer every `FlushSeconds` in batches of `BatchSize`, purges entries older than `RetentionDays` once a day, and flushes on shutdown. |
| `IRightAuditQuery` / `RightAuditFilter` (in `OoBDev.AspNetCore.Abstractions`) | The swappable read contract; `MapRightAuditQuery(rights)` in `OoBDev.AspNetCore.Mvc` exposes it as a right-guarded `GET /api/v1/right-audit` |
| `EfRightAuditStore` | The default `IRightAuditQuery`: `AppendAsync`, `QueryAsync(filter)` (or `QueryAsync(subject, from, maxCount)`) newest first, `PurgeAsync(olderThan)`. Over-long values are clipped to the column length. |
| `RightAuditDbContext` / `RightAuditRow` | The `RightAudit` table (time stored as UTC ticks), indexed by time and by subject and time. |

```csharp
services.TryAddRoleRightMapping(configuration);
services.TryAddEntityFrameworkRightAudit(configuration, (sp, o) => o.UseSqlite("Data Source=audit.db"));
services.AddRightAuditLog();   // after both: logs and hands each check to the sink
```

Options (`OoBDev:RightAudit`): `RetentionDays` 365, `BatchSize` 100, `QueueCapacity` 10000, `FlushSeconds` 5.

The consumer picks the provider and creates the schema (`EnsureCreated` or its own migrations; shipped migrations are not built). Not built: a right-guarded query endpoint and screen; hash chaining of entries.

Tests: `OoBDev.AspNetCore.EntityFrameworkCore.Tests` (Simulate and Unit, Sqlite in memory). Not run: SQL Server and PostgreSQL.
