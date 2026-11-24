# OoBDev.ComplexEvents.Hosting

Hosted service that runs due items of an `IScheduleStore` (see [OoBDev.ComplexEvents](../OoBDev.ComplexEvents/README.ComplexEvents.md)).

```csharp
services.TryAddEntityFrameworkScheduleStore((sp, o) => o.UseSqlServer(connectionString)); // optional, in memory otherwise
services.TryAddScheduleHosting(async (item, ct) => { /* deserialize by item.EventType, dispatch */ },
    o => o.PollInterval = TimeSpan.FromSeconds(15));
```

- `ScheduleHost`: polls on a `PeriodicTimer` (so `FakeTimeProvider` drives tests), leases up to `BatchSize` due items for `Lease`, completes the ones the callback finishes, and logs and retries on the next interval when a poll throws. A throwing callback leaves the lease to expire and counts an attempt.
- `ScheduleHostOptions`: `PollInterval` (30 s), `BatchSize` (20), `Lease` (5 min).
- Logging comes from the host application.
