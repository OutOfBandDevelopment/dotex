# OoBDev.PostgreSql.WorkQueue

The PostgreSQL provider of [OoBDev.WorkQueue](../../../Framework/OoBDev.WorkQueue/README.WorkQueue.md). Items live in one row each, so a screen or a SQL query sees status, attempts, lease and retry time. Design: [docs/design/WorkQueue](../../../../docs/design/WorkQueue/README.md).

## Use

```csharp
services.TryAddPostgresWorkQueue(configuration);   // WorkQueue:* and WorkQueue:Postgres:*
services.AddWorkStage<HashHandler>("hash");

await queue.MigrateAsync(options);                  // creates the schema and tables, safe to repeat
```

`PostgresWorkQueueSchema.CreateScript(options)` returns the same script as text for a migration tool.

## Rules

- A claim is one transaction: due rows that used their attempts are swept to `parked` (hung) or `dead` with a dead-letter row, then up to `batch` due rows are taken with `FOR UPDATE SKIP LOCKED`, so concurrent workers never share an item.
- Every later call matches `status = running`, `attempts` and `claimed_by`; a stale worker changes nothing.
- All times are passed from `TimeProvider`, never `now()`, so tests drive the clock.
- Claim order is `IPostgresClaimOrder` (default `OldestFirstClaimOrder`: due time, then key). The returned text goes into the query, so it must come from code.

## Options (`WorkQueue:Postgres`)

| Key | Default | Meaning |
|-----|---------|---------|
| `ConnectionString` | required | Database holding the queue |
| `Schema` | `work_queue` | Schema of the tables |
| `TasksTable` | `tasks` | One row per item (primary key `stage, item_key`) |
| `DeadLettersTable` | `dead_letters` | One row per death, with `redriven_at` |

Names match `^[a-z_][a-z0-9_]{0,62}$` and are quoted into the SQL.

## Worker events

`TryAddPostgresWorkerEvents(configuration)` registers `PostgresWorkerEvents` as the `IWorkerEvents`. It uses the same connection string and schema as the queue and two more tables (`EventsTable` default `worker_events`, `EventReadersTable` default `worker_events_reader`; the migration script creates them with the task tables). Retention binds from `WorkQueue:Events`.

## Tests

Unit tests cover the script, identifier validation and registration. Integration tests run against the `pgvector` service of the test stack (`PGVECTOR_CONNECTION_STRING`) in a throwaway schema per test.
