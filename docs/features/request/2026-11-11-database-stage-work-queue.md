# Feature request: database-backed stage work queue with leases, throttling and worker events

- **Status:** new
- **Requested by:** owner (filed from the NAS storage catalog application, `C:\repo\_storage`, `src/Storage.Work` and `src/Storage.Workers`)
- **Date:** 2026-11-11
- **Area:** `OoBDev.MessageQueueing` neighbours; the queue and event-stream answer in `2026-11-10-postgres-support.md`

## Need

Long-running, restartable pipelines (hash, extract, OCR, embed, thumbnail and more, per file) need a queue where the unit of work is a row, not only a message: status, attempts, a lease, a runtime ceiling and a retry time must be visible to a screen and to SQL. A broker queue gives delivery but not that view, and not the rule "a worker that lost its lease cannot overwrite the new owner".

The catalog application built this on PostgreSQL and runs it on about 570,000 files. `2026-11-10-postgres-support.md` asks for "a queue and event-stream answer for Postgres in place of Service Broker"; this request is a concrete, working answer to it.

## Proposed behavior

A work-queue abstraction with a database provider (PostgreSQL first), used by stage runners hosted in the generic host:

1. **Claim** with `FOR UPDATE SKIP LOCKED`; sets running, attempts + 1, claimed by, claimed at, lease until.
2. **Complete, fail and heartbeat** match `(attempt, claimed_by)`, so a stale worker is ignored.
3. **Stage SLA** per stage: lease (maximum silence), maximum runtime (ceiling even with heartbeats), maximum attempts; a retryable failure returns the item to pending with exponential back-off, and an exhausted item is parked, along with its dependent items.
4. **Claimable count**, not pending count, so a supervisor starts a GPU or container for a stage only while a claim would succeed.
5. **Owner controls** per stage: pause and speed (a row an operator screen can change).
6. **Worker events:** workers publish progress to an event queue; one consumer appends to a history table; each reader keeps its own high-water mark; retention by record count and age.
7. **Idempotent stage rule** documented as the contract: outputs keyed by content hash and written by upsert, so at-least-once delivery is safe.

```csharp
services.AddStageWorkers(o => o.UsePostgres(connection))
        .AddStage<HashStage>(sla: new(Lease: 5.Minutes(), MaxRuntime: 1.Hours(), MaxAttempts: 5))
        .AddDuty<RecountTagsDuty>();   // periodic work that is not per item
```

A stage or duty is switched on by name in configuration (`Workers:Stages:hash = true`), so one host owns each stage and moving a stage between hosts is a settings change.

## Acceptance criteria

- Given two workers claiming at once, when the queue holds one item, then exactly one claim succeeds.
- Given a worker whose lease expired and a second worker that claimed the item, when the first calls complete, then the call is rejected and the second's result stands.
- Given an item that fails `MaxAttempts` times, when the last fails, then it is parked with its dependents and retried once after the cool-off only when the queue is empty.
- Given a stage with nothing claimable, when the supervisor asks, then the claimable count is zero even if rows are pending for a later retry time.
- Given an events consumer that restarts, when it resumes, then it continues from its stored high-water mark with no duplicates in the history table.

## Existing code checked

- `OoBDev.MessageQueueing` and its designs (`docs/design/MessageQueuing`): broker providers behind a context-based interface; the delivery side. The row view, leases and per-stage SLA are not there as far as the design pages show; the code was not read.
- `2026-11-10-complex-events-and-scheduling.md` and `2026-11-10-event-document-history-stores.md`: neighbours (scheduling, event history); not compared in detail.
- The catalog application documents the behavior as rules `que-1` to `que-14` in `docs/features/pipeline/queue-and-stages.md`; the tests live with the application.

## Security and operations

The queue tables hold paths and statuses, not content. The operator controls (pause, speed) are writes and need an application right. Database cost: one index per stage state; the events table is capped by the retention settings.

## Priority

- **Impact:** a reusable restartable pipeline host; replaces ad hoc job tables.
- **Needed by:** no date.
- **Workaround today:** the catalog application owns its queue.

## Open questions

- Keep the PGMQ-style message queue beside the task rows (the application does, for wake-ups) or use `LISTEN`/`NOTIFY` for the wake-up and rows only (recommended to decide after a spike; the application has measured only the first)?
- Should the table shape be fixed by the library or mapped by the application?

## Triage notes
