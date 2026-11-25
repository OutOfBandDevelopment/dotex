# CQRS and Event Sourcing — Architecture

[← Requirements](requirements.md) · [↑ Overview](README.md) · [API design →](api-design.md)

**Status:** Draft · **Date:** 2026-11-11

## Contents

- [Write and read sides](#write-and-read-sides)
- [Sources](#sources)
- [Relation to Complex Events](#relation-to-complex-events)

### List of Figures

1. [Figure 1 — Write and read sides](#figure-1)

### List of Tables

1. [Table 2 — Sources and what they teach](#table-2)
2. [Table 3 — Relation to Complex Events](#table-3)

## Write and read sides

A command handler loads an aggregate by replaying its stream from the event store, the aggregate raises events, and the store appends them with an expected sequence. A relay reads new events from the store by global position and sends them through the existing message-queue sender. A projection is a message-queue handler applies each event to a read model in the entity store, and saves a checkpoint. Queries read the entity store only.

The event store, the event stream and the entity store are three separate parts, each swapped by configuration (see [API design](api-design.md#registration)); the event stream is the existing message-queue provider set, not a new contract. They share no storage, so the relay, not a dual write, is what keeps them consistent: events are durable in the store first, and the stream is a delivery mechanism that can be rebuilt from the store.

<a id="figure-1"></a>

```plantuml
@startuml
skinparam shadowing false
skinparam rectangle {
  BackgroundColor White
}
rectangle "Command handler\n[IHandler<TCommand, TResult>]" as H
rectangle "Aggregate\n[replays events, raises events]" as A
database "Event store
[provider: append-only, expected sequence]" as S
rectangle "Event relay
[store position to stream]" as X
queue "Event stream
[existing message-queue providers]" as M
rectangle "Projection
[IMessageQueueHandler, checkpoints]" as P
database "Entity store
[provider: read models, eventually consistent]" as R
rectangle "Query handler\n[IHandler<TQuery, TResult>]" as Q
H --> A : load and apply
A --> S : append events
S --> X : read by position
X --> M : publish
M --> P : deliver
P --> R : update
Q --> R : read
@enduml
```

*Figure 1 — Write and read sides*

## Sources

<a id="table-2"></a>**Table 2 — Sources and what they teach**

| Source (read-only, `C:\repo\learning`) | Teaches | Used for |
|------|---------|----------|
| `fohjin-cqrs` | Pattern explanations from first principles, aggregate, repository, unit of work, compensating transaction | The teaching baseline; linked, not copied |
| `event-sourcing` | Append-only store, schema registry, follow API, projections, upcasting, hash-chained tamper evidence, optimistic concurrency | The infrastructure target and the optional extensions |
| `sync-mesh` | Hybrid logical clock; event lineage as descriptive provenance | Ordering across skewed clocks (R8) |

## Relation to Complex Events

The SharedFramework epic "Complex Events" (event sourcing, CQRS, scheduling, persistence, EventHub) is a proposal from the earlier consolidation. This design takes over its event-sourcing and CQRS parts and leaves the rest where they belong.

<a id="table-3"></a>**Table 3 — Relation to Complex Events**

| Complex Events feature | Decision |
|------------------------|----------|
| Event sourcing abstractions and core | Replaced by this design's contracts |
| Event persistence (EF Core provider) | A store provider behind the same contract, built when a consumer appears |
| Event scheduling (cron attribute) | Separate scheduling feature, not part of event sourcing |
| Azure EventHub provider | A transport adapter in the cloud integrations, not in the core |

[API design →](api-design.md)
