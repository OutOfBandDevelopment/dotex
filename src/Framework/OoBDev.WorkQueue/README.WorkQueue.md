# OoBDev.WorkQueue

A work queue whose unit of work is a visible row: status, attempts, lease, runtime ceiling and retry time are readable by a screen and by SQL, and a worker that lost its lease cannot overwrite the new owner. A broker (see `OoBDev.MessageQueueing`) delivers messages; this queue tracks restartable stages. Design: [docs/design/WorkQueue](../../../docs/design/WorkQueue/README.md).

This project has no database reference. The PostgreSQL provider is [OoBDev.PostgreSql.WorkQueue](../../ExternalServices/PostgreSql/OoBDev.PostgreSql.WorkQueue/README.PostgreSql.WorkQueue.md); `InMemoryWorkQueue` is the process-local provider for tests and tools.

## Use

```csharp
services.TryAddWorkQueue(configuration);   // binds WorkQueue:*, registers TimeProvider
services.TryAddInMemoryWorkQueue();        // or the PostgreSQL provider
services.AddWorkStage<HashHandler>("hash");
```

A handler implements `IWorkStageHandler.HandleAsync(WorkItem, CancellationToken)`. Return to complete the item, throw to record a failed attempt, throw `NonRetryableWorkException` to dead-letter at once. Outputs must be keyed by the item and written by upsert, because delivery is at least once.

## Rules

- A claim sets `running`, adds one attempt and a lease. After the lease (or the runtime ceiling) the item can be claimed again and the attempt counts.
- Every call after the claim matches `(attempt, worker)`: `HeartbeatAsync`, `CompleteAsync` and `FailAsync` return false or null for a stale worker and change nothing.
- A retryable failure returns the item to `pending` after `BackoffBase` doubled per attempt, capped at `BackoffMax`. An item that used its attempts leaves the flow: `parked` if it hung (lease expired), `dead` if it kept failing, each with a dead-letter record. `RedriveAsync` returns them with a fresh budget.
- `StatsAsync().Claimable` (not the row count) tells a supervisor whether a stage is wanted.
- Time comes from `TimeProvider`. Logs carry stage, key and counts only.

## Options (`WorkQueue`)

| Key | Default | Meaning |
|-----|---------|---------|
| `Default:Lease` | 15 minutes | Silence allowed before the claim expires |
| `Default:MaxRuntime` | 2 hours | Ceiling for one attempt even with heartbeats |
| `Default:MaxAttempts` | 3 | Claims before the item leaves the flow |
| `Default:Enabled`, `Stages:{name}:Enabled` | true | A stage worker starts only when enabled |
| `Stages:{name}:*` | none | Per-stage override of the same keys |
| `BackoffBase`, `BackoffMax` | 30 seconds, 1 hour | Retry delay schedule |
| `BatchSize` | 1 | Items claimed at once |
| `PollInterval` | 5 seconds | Wait when a claim finds nothing |
| `HeartbeatInterval` | a third of the lease | How often the worker extends the lease |

## Worker events (`WorkQueue:Events`)

`IWorkerEvents` keeps the progress history of the workers: `PublishAsync(source, message)` appends, `ReadAsync(reader, limit, fromStart)` returns what a reader has not seen and moves its mark (a new reader starts at the end), `TrimAsync()` applies the retention. `TryAddInMemoryWorkerEvents(configuration)` registers the in-process store; the PostgreSQL store is in `OoBDev.PostgreSql.WorkQueue`.

| Option | Default | Meaning |
|--------|---------|---------|
| `MaxRecords` | 10000 | Events kept; older ones are trimmed |
| `MaxAge` | 7 days | Longest an event is kept; readers idle longer are dropped |

Not taken: a PGMQ reader (the rows are the source of truth; see the design).
