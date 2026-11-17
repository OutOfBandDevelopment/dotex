# OoBDev.Communications.EntityFrameworkCore

A durable `ISuppressionList` in a relational database, so bounces, complaints and unsubscribes survive restarts and are shared by every instance.

| Type | Role |
|------|------|
| `SuppressionsDbContext` / `SuppressionRow` | The `Suppressions` table keyed by the lower-cased address, with reason and added time. |
| `EfSuppressionList` | `ISuppressionList`: add (the first reason stays when two writers race), remove, check. Addresses compare case-insensitively. |

```csharp
services.TryAddEntityFrameworkSuppressionList((sp, o) => o.UseSqlite("Data Source=suppress.db"));
services.AddEmailSuppression(); // wraps ISendService; uses this list instead of the in-memory one
services.AddUnsubscribeLinks();  // OoBDev.Communications: signed RFC 8058 links feeding the same list
```

Register it before `AddEmailSuppression`. The consumer picks the provider and creates the schema (`EnsureCreated` or its own migrations; shipped migrations are not built).

Tests: `OoBDev.Communications.EntityFrameworkCore.Tests` (Simulate, Sqlite in memory). Not run: SQL Server and PostgreSQL.
