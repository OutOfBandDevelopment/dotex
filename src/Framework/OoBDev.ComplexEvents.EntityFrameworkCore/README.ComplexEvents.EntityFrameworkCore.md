# OoBDev.ComplexEvents.EntityFrameworkCore

Database `IScheduleStore` for [OoBDev.ComplexEvents](../OoBDev.ComplexEvents/README.ComplexEvents.md). The consumer picks the EF Core provider.

```csharp
services.TryAddEntityFrameworkScheduleStore((sp, o) => o.UseSqlServer(connectionString));
```

- `EfScheduleStore`: register is an upsert by key; a lease is one conditional `UPDATE`, so two instances never take the same item; items past the attempt limit are marked failed (`FailedItemsAsync`).
- `ScheduleDbContext` / `ScheduleRow`: table `ComplexEventSchedule`, unique `Key`, index on `(Failed, DueAtTicks)`; times are UTC ticks.
- `LastRunRow` / `EfLastRunStore`: table `ComplexEventLastRun` (key, last run ticks), the durable `ILastRunStore` registered by `TryAddEntityFrameworkScheduleStore`; an older value never replaces a newer one.
- Tests run on in-memory SQLite (`Simulate`); SQL Server is not exercised yet.
