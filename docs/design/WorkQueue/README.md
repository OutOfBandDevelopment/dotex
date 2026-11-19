# Database work queue for restartable stages

Status: **first slice built 2026-11-16** (Framework `OoBDev.WorkQueue` with the in-memory provider and stage workers, PostgreSQL adapter `OoBDev.PostgreSql.WorkQueue`; placement confirmed by the owner 2026-11-12, option A). Worker events built 2026-11-17 (`IWorkerEvents`, in-memory and PostgreSQL stores); the PGMQ reader was not taken (rows are the source of truth) and application stages stay in the catalog. Request: [database stage work queue](../../features/completed/2026-11-11-database-stage-work-queue.md). Related: [postgres support](../../features/request/2026-11-10-postgres-support.md).

## Contents

1. [Need](#need)
2. [Superset comparison](#superset-comparison)
3. [Design](#design)
4. [Migration plan](#migration-plan)
5. [Testing strategy](#testing-strategy)
6. [Not in the first slice](#not-in-the-first-slice)

## Need

Long-running, restartable pipelines need a queue where the unit of work is a visible row: status, attempts, lease, runtime ceiling and retry time readable by a screen and by SQL, and a worker that lost its lease cannot overwrite the new owner. A broker gives delivery but not that view.

## Superset comparison

**Table 1 — Sources compared**

| Source | What it has | Verdict |
|--------|-------------|---------|
| dotex `OoBDev.MessageQueueing` (+ Abstractions, Hosting) | Broker message queues (SQS, Service Bus, RabbitMQ adapters) with handlers and an AsyncAPI document | Delivery only; no row-level status, lease or attempt fencing. Stays the answer for messaging |
| dotex `OoBDev.EventSourcing.Postgres` | PostgreSQL event store | Connection and schema conventions to follow; not a work queue |
| Catalog `WorkQueue` (361 lines) | Claim with `FOR UPDATE SKIP LOCKED`, sweep of exhausted tasks into `dead_letters` (parked if it hung, dead if it kept failing), heartbeat, complete and fail fenced by `(attempt, claimed_by)`, optional PGMQ reader | Base for the PostgreSQL provider |
| Catalog `StageSla`, `WorkOptions` | Lease (15 min), max runtime (2 h), max attempts (3) per stage; options with per-stage overrides | Base for `StageSla` and the options |
| Catalog `WorkerEvents`, `WorkerEvent` (113 lines) | Event queue, one consumer appends history, per-reader high-water mark, retention by count and age | Base for the events slice |
| Catalog `TaskEnqueuer`, `StageStats`, `WorkItem` | Enqueue, claimable counts, the claim record | Base |
| Catalog `Order(stage)` | A hard-coded caption ordering over catalog tables (`caption_requests`, `files`) | Catalog-specific: becomes a pluggable `IClaimOrder` the application supplies |
| Catalog stage runners, duties, owner stage control | Application stages over catalog tables | Stay in the catalog; the library offers the hosting shape only |

## Design

- `IWorkQueue` (`ClaimAsync`, `HeartbeatAsync`, `CompleteAsync`, `FailAsync`, `ClaimableCountAsync`, `EnqueueAsync`) with the fence rule: every call after the claim matches `(attempt, claimed_by)` and a stale worker is ignored.
- A retryable failure returns the item to pending with exponential back-off; an exhausted item is parked (it hung) or dead (it kept failing) with a dead-letter row, along with its dependents; a redrive operation returns them.
- Claimable count (not pending count) drives a supervisor.
- Per-stage owner controls (pause, speed) are a table the application's screen can edit; the library reads them.
- Table and column names are options (schema, task table name), not catalog names; the migration script is generated from the options.
- The PGMQ reader is optional and off by default; the table is always the source of truth.
- `AddStageWorkers(...)`, `AddStage<T>(sla)` and `AddDuty<T>()` register hosted services through `TryAdd*`; a stage is enabled by name in configuration (`Workers:Stages:hash`).
- Stage contract documented: outputs keyed by content hash and written by upsert, so at-least-once is safe.
- Time comes from `TimeProvider`. Logging carries ids and counts only.

## Migration plan

**Table 2 — Projects**

| Project | Current location | Target location | Namespace | Package or unit |
|---------|------------------|-----------------|-----------|-----------------|
| Contracts, SLA, options, hosting shape | `C:\repo\_storage\src\Storage.Work` and `Storage.Workers` hosting (read-only reference) | new `src/Framework/OoBDev.WorkQueue` | `OoBDev.WorkQueue` | packable, no database reference |
| PostgreSQL provider and migration | `WorkQueue.cs`, `TaskEnqueuer.cs`, `WorkerEvents.cs` | new `src/ExternalServices/PostgreSql/OoBDev.PostgreSql.WorkQueue` (grouped with the pgmq adapter) | `OoBDev.PostgreSql.WorkQueue` | packable adapter, Npgsql only here |
| Tests | `tests/Storage.Api.Tests`, `Storage.Catalog.IntegrationTests` | `OoBDev.WorkQueue.Tests` (fake clock, in-memory fake provider, `Unit`) and `OoBDev.PostgreSql.WorkQueue.Tests` (`Unit` for script, options and registration; `Integration` against the `pgvector` service) | matching | MSTest |

**Table 3 — References that change**

| Project | Change |
|---------|--------|
| Test stack | gained the `pgvector` PostgreSQL service (2026-11-16) |
| Storage catalog | drops `Storage.Work` and references the packages (outside this repository) |

## Testing strategy

Unit (fake clock and in-memory provider): one claim wins when two workers race; a completion from a worker whose lease expired is rejected; the retry time follows the exponential schedule; the third failure parks or kills as the case may be; claimable count is zero when only future retries are pending. Integration (PostgreSQL): the same cases against real `SKIP LOCKED`, plus the events consumer resuming from its stored mark without duplicates. Target 85 to 90 percent coverage.

## Not in the first slice

- The PGMQ reader (not taken: the table is the source of truth and the wake-up is polling).
- Application stages and duties (stay in the catalog).

## Built

- Worker events 2026-11-17: `IWorkerEvents` (`PublishAsync`, `ReadAsync` with a per-reader mark, `TrimAsync`), `WorkerEventsOptions` (`WorkQueue:Events`: `MaxRecords`, `MaxAge`), `InMemoryWorkerEvents`, `PostgresWorkerEvents` (tables `worker_events` and `worker_events_reader`, names in `PostgresWorkQueueOptions`), `TryAddInMemoryWorkerEvents` and `TryAddPostgresWorkerEvents`.
