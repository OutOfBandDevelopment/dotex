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

- **Providers:** the stores (`EventSourcingDbContext`, the two store classes and the model) live in `OoBDev.EventSourcing.EntityFrameworkCore` and reference only EF Core and the Abstractions. The database packages are separate small projects (`.Postgres`, `.SqlServer`, `.Sqlite`), each registering its provider under a kebab-case key (`postgres`, `sql-server`, `sqlite`) in its own `{Vendor}Globals`, so a deployment pulls only the database it uses.
- **JSON columns:** `jsonb` on PostgreSQL, `nvarchar(max)` with `ISJSON` on SQL Server, `TEXT` on SQLite; the model maps one string property and the provider project configures the column type.
- **Migrations:** one migration set per database, in the provider project. SQLite and tests may use `EnsureCreated` through an option; production deployments apply migrations.
- **Gaps in the global position:** identity values are allocated before commit, so a concurrent transaction can commit a lower position after a higher one is visible. The relay therefore reads only positions older than a small safety window (default 5 seconds) and re-reads gaps once; this is a known limitation of every identity-based feed.
- **Not in the first cut:** querying inside the JSON value (`QueryAsync` filters on `Type` and `Key` prefix only) and sharding by stream.

[Testing strategy →](testing-strategy.md)

**Status:** Draft · **Date:** 2026-11-11 · *Proposed: none of these types exist yet.*

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
| `IEventStream` | Delivery of committed events: `PublishAsync(EventEnvelope)` and `SubscribeAsync(name, handler)`; at-least-once, ordered per stream id, consumers deduplicate by stream id and sequence. Provider: `IEventStreamProvider` / `IEventStreamFactory` | R5 |
| `IEntityStore` | Read-model and entity state: `GetAsync(type, key)`, `SaveAsync(type, key, value, expectedVersion)`, `DeleteAsync` and `QueryAsync`; also holds projection checkpoints. Provider: `IEntityStoreProvider` / `IEntityStoreFactory` | R5, R6 |
| `EventRelay` | Hosted service that reads the event store by global position and publishes to the event stream, saving its checkpoint in the entity store; this keeps the store and the stream independent (no dual write) | R5, R6 |
| `EventEnvelope` | Stream id, sequence, type name, timestamp, payload | R1 |
| `AggregateRoot` | Applies events to change state, collects uncommitted events, rebuilds from history | R2 |
| `IAggregateRepository<T>` | Loads an aggregate by replay and saves its uncommitted events through `IEventStore` | R2, R3 |
| `IProjection` | `ApplyAsync(EventEnvelope)` updates one read model | R5 |
| `IEventUpcaster` | Converts an older event payload to the current shape on read | R7 |

Commands and queries use the existing `IHandler<TRequest, TResponse>`; no new handler contract is added (R4). Time uses `TimeProvider` (R8).

## Registration

The three infrastructure parts are separate providers so each can be swapped by design or deployment (owner, 2026-11-11). `AddEventSourcing()` registers in-memory defaults with `TryAdd*`, and each part is selected by its own configured key, as the other keyed providers do:

```json
{ "OoBDev": { "EventSourcing": {
    "EventStore":  { "Type": "sql-server" },
    "EventStream": { "Type": "rabbitmq" },
    "EntityStore": { "Type": "mongodb" } } } }
```

Each adapter lives in its own project and references the Abstractions only. The event stream adapters reuse the existing message-queue adapters (RabbitMQ, SQS, Service Bus, SQL Service Broker); the entity store adapters reuse the existing database clients. A deployment can therefore mix, for example, SQL Server for events, RabbitMQ for the stream and Redis for entities.

[Testing strategy →](testing-strategy.md)
