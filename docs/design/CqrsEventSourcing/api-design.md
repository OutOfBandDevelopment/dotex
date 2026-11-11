# CQRS and Event Sourcing — API Design

[← Architecture](architecture.md) · [↑ Overview](README.md) · ## Entity Framework stores

The event store and the entity store have a relational implementation on Entity Framework Core, so one model serves several databases (owner, 2026-11-11). The first databases are **PostgreSQL, SQL Server and SQLite**, each a configurable option. The event stream is not relational; it stays on the message-queue adapters.

```json
{ "OoBDev": { "EventSourcing": {
    "EventStore":  { "Type": "entity-framework" },
    "EntityStore": { "Type": "entity-framework" },
    "EntityFramework": { "Database": "postgres", "ConnectionName": "EventSourcing" } } } }
```

`Database` is `postgres`, `sql-server` or `sqlite`. The two stores may use different databases or connection names by giving each its own `EntityFramework` section (`EventStore:EntityFramework`, `EntityStore:EntityFramework`).

| Table | Columns | Notes |
|-------|---------|-------|
| `Events` | `Position` (identity, global order), `StreamId`, `Sequence`, `Type`, `Timestamp`, `Payload` (JSON), `Metadata` (JSON) | unique index on (`StreamId`, `Sequence`); a duplicate-key error is the optimistic concurrency conflict |
| `Entities` | `Type`, `Key`, `Version`, `Value` (JSON) | primary key (`Type`, `Key`); `Version` is the concurrency token |
| `Checkpoints` | `Name`, `Position` | one row per projection or relay |

- **Providers (layout confirmed by the owner, 2026-11-11):** all in Framework: the stores (`EventSourcingDbContext`, the two store classes and the model) live in `OoBDev.EventSourcing.EntityFrameworkCore` and reference only EF Core and the Abstractions. The database packages are separate small projects (`.Postgres`, `.SqlServer`, `.Sqlite`), each registering its provider under a kebab-case key (`postgres`, `sql-server`, `sqlite`) in its own `{Vendor}Globals`, so a deployment pulls only the database it uses.
- **JSON columns:** `jsonb` on PostgreSQL, `nvarchar(max)` with `ISJSON` on SQL Server, `TEXT` on SQLite; the model maps one string property and the provider project configures the column type.
- **Schema delivery (owner, 2026-11-11):** PostgreSQL and SQLite use EF Core migrations, one set per database in the provider project. SQL Server uses a SQL project that builds a dacpac, like `OoBDev.Example.DB` (`MSBuild.Sdk.SqlProj`): `OoBDev.EventSourcing.SqlServer.Database` holds the `Events`, `Entities` and `Checkpoints` table scripts and is deployed with DacFx (see [DesiredStateDatabase](../DesiredStateDatabase/README.md)), so it has no EF migrations. SQLite and tests may use `EnsureCreated` through an option. A parity test (Integration, SQL Server container) deploys the dacpac and checks that the EF model's tables, columns, types and indexes match it, so the two cannot drift.
- **Gaps in the global position:** identity values are allocated before commit, so a concurrent transaction can commit a lower position after a higher one is visible. The relay therefore reads only positions older than a small safety window (default 5 seconds) and re-reads gaps once; this is a known limitation of every identity-based feed.
- **Not in the first cut:** querying inside the JSON value (`QueryAsync` filters on `Type` and `Key` prefix only) and sharding by stream.

## Event stream on the message queues

Events travel as ordinary queue messages, so everything already built for queues applies: `MessageQueueAttribute` channels, `IMessageContext`, the receiver host, retry and dead-letter behaviour of each adapter, and the AsyncAPI contributors. Nothing event-specific is added to the queue abstractions.

| Queue | Adapter | State |
|-------|---------|-------|
| Azure Service Bus | `OoBDev.Microsoft.Azure.ServiceBus` | exists |
| Azure Storage Queues | `OoBDev.Microsoft.Azure.StorageAccount` | exists |
| SQL Server Service Broker | `OoBDev.Microsoft.SqlServer.ServiceBroker` | exists |
| PostgreSQL pgmq | `OoBDev.PostgreSql.Pgmq` (new, ExternalServices) | to build |
| RabbitMQ, Amazon SQS | existing adapters | exist; not part of the first target set |

The first target set is the Azure queues, Service Broker and pgmq. pgmq is the PostgreSQL extension that stores queues as tables and exposes `pgmq.send`, `pgmq.read` (with a visibility timeout), `pgmq.archive` and `pgmq.delete`; the adapter calls these functions through Npgsql and maps visibility timeout to the same retry behaviour as the other adapters.

## CloudEvents at the edge

CloudEvents 1.0 is an edge option, not the internal envelope (owner, 2026-11-11). `EventRecord` stays the internal shape. A mapper converts between `EventRecord` (with its `IMessageContext`) and a CloudEvent in structured JSON mode (`application/cloudevents+json`) where events cross a boundary: an HTTP endpoint or webhook, or a queue adapter option that sets the content type. `id` is the stream id and sequence, `source` is the configured application name, `type` is the event type name, `time` is the timestamp, and `subject` is the stream id. Incoming CloudEvents are mapped to a queue message the same way, so external producers use the normal handlers.

[Testing strategy →](testing-strategy.md)

**Status:** Draft · **Date:** 2026-11-11 · *Built (2026-11-11): `IEventStore`, `EventRecord`, `NewEvent`, `AppendResult`, `IEntityStore`, `EntityRecord`, `EventsChannel`, the in-memory stores, `EventRelay` and `TryAddEventSourcing()`. Proposed, not built: the `.SqlServer` package and dacpac, `AggregateRoot`, `IAggregateRepository<T>`, `IProjection`, `IEventUpcaster` and the CloudEvents mapper. Also built: `EfEventStore`, `EfEntityStore` and `EventSourcingDbContext` in `OoBDev.EventSourcing.EntityFrameworkCore`, and the `.Sqlite` and `.Postgres` database packages with `InitialCreate` migrations (tested on SQLite; PostgreSQL checked only for migration-to-model drift). Provider and factory interfaces are replaced by keyed services selected with `TryAddConfiguredKeyedService`.*

## Contents

- [Contracts](#contracts)
- [Registration](#registration)

### List of Tables

1. [Table 4 — Proposed contracts](#table-4)

## Contracts

<a id="table-4"></a>**Table 4 — Proposed contracts**

| Type | Role | Requirement |
|------|------|-------------|
| `IEventStore` | Durable history: `AppendAsync(streamId, expectedSequence, events)`, `ReadAsync(streamId, fromSequence)` and `ReadAllAsync(fromPosition)` for the global feed; returns a conflict result on sequence mismatch. Provider: `IEventStoreProvider` / `IEventStoreFactory` | R1, R3 |
| Event stream | No new contract: the existing message-queue pattern. `EventRelay` sends through `IMessageQueueSender<EventsChannel>`; a projection is an `IMessageQueueHandler<EventsChannel, EventRecord>` receiving an `IMessageContext`. The queue is chosen by the existing sender and receiver provider factories and configuration | R5 |
| `IEntityStore` | Read-model and entity state: `GetAsync(type, key)`, `SaveAsync(type, key, value, expectedVersion)`, `DeleteAsync` and `QueryAsync`; also holds projection checkpoints. Provider: `IEntityStoreProvider` / `IEntityStoreFactory` | R5, R6 |
| `EventRelay` | Hosted service that reads the event store by global position and sends the events through the message-queue sender, saving its checkpoint in the entity store; this keeps the store and the stream independent (no dual write) | R5, R6 |
| `EventRecord` | Stream id, sequence, type name, timestamp, payload; correlation and caller information travel in the `IMessageContext`, as for every other message | R1 |
| `AggregateRoot` | Applies events to change state, collects uncommitted events, rebuilds from history | R2 |
| `IAggregateRepository<T>` | Loads an aggregate by replay and saves its uncommitted events through `IEventStore` | R2, R3 |
| `IProjection` | `ApplyAsync(EventEnvelope)` updates one read model | R5 |
| `IEventUpcaster` | Converts an older event payload to the current shape on read | R7 |

Commands and queries use the existing `IHandler<TRequest, TResponse>`; no new handler contract is added (R4). Time uses `TimeProvider` (R8).

## Registration

The three infrastructure parts are separate providers so each can be swapped by design or deployment (owner, 2026-11-11). `AddEventSourcing()` registers in-memory defaults with `TryAdd*`, and the store and the entity store are selected by their own configured key, and the stream by the existing message-queue provider configuration, as the other keyed providers do:

```json
{ "OoBDev": { "EventSourcing": {
    "EventStore":  { "Type": "sql-server" },
    "EntityStore": { "Type": "mongodb" } } } }
```

Each adapter lives in its own project and references the Abstractions only. The event stream uses the existing message-queue adapters unchanged; the entity store adapters reuse the existing database clients. A deployment can therefore mix, for example, SQL Server for events, RabbitMQ for the stream and Redis for entities.

[Testing strategy →](testing-strategy.md)
