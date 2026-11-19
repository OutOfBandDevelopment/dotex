# Complex events and scheduling

Status: **proposed design, written 2026-11-19 on the assumed answer A of Q18; no code, built when a consumer appears.** Request: [complex events, scheduling and Event Hubs](../../features/request/2026-11-10-complex-events-and-scheduling.md). Epic 8 of [SharedFramework](../SharedFramework/README.md).

## Contents

1. [Scope](#scope)
2. [Requirements](#requirements)
3. [Architecture](#architecture)
4. [API sketch](#api-sketch)
5. [Scheduling rules](#scheduling-rules)
6. [Testing strategy](#testing-strategy)
7. [Open points](#open-points)

### List of Figures

1. Figure 1 — Components and layers

### List of Tables

1. Table 1 — Requirements
2. Table 2 — Test plan

## Scope

In: an attribute-declared event handler model, a scheduler with persisted state, and an Event Hubs provider behind an interface. Out: event sourcing (aggregates, streams and projections stay in [CqrsEventSourcing](../CqrsEventSourcing/README.md)); the SharedFramework `ComplexEvents.DatabaseExtensions` SQL project (no database project, assumed answer A of Q18); copying the owner's `event-sourcing` and `sync-mesh` material (needs the owner's say-so).

The handler model dispatches in-process events. It is not a second queue: durable delivery stays with `OoBDev.MessageQueueing`.

## Requirements

**Table 1 — Requirements**

| Id | Requirement | Acceptance |
|----|-------------|------------|
| CE-1 | A class marked `[ComplexEventHandler]` for an event type is found by a resolver and receives matching events | resolver returns the handler; a non-matching event is not dispatched |
| CE-2 | Several handlers per event run in declared order; one failing handler does not stop the others, its error is reported together | all run; an aggregate failure lists each error |
| CE-3 | A schedule registers an event for a due time (`ScheduleAt`) and runs it once | runs at or after the due time, then is released |
| CE-4 | A restart does not run a released schedule again, and two instances never run the same item | lease with an expiry; completion is recorded in the same transaction as the release |
| CE-5 | Schedule state is persisted through EF Core (SQLite, SQL Server, PostgreSQL) | same tests pass on each provider |
| CE-6 | An Event Hubs provider publishes and consumes through `IEventHubClient`, with a fake for tests | no live Azure in Unit or Simulate; live tests are `LiveIntegration` |
| CE-7 | Connection strings and options come from validated options (`AddValidatedOptions<T>()`) | invalid options fail at start |

*Table 1 lists what must hold; CE-4 is the one that needs a lease and an idempotent handler.*

## Architecture

```plantuml
@startuml
skinparam componentStyle rectangle
package "Common" {
  [OoBDev.ComplexEvents.Abstractions] as Abs
}
package "Framework" {
  [OoBDev.ComplexEvents] as Core
  [OoBDev.ComplexEvents.EntityFrameworkCore] as Ef
}
package "ExternalServices" {
  [OoBDev.Microsoft.Azure.EventHub] as Hub
}
Core --> Abs
Ef --> Abs
Hub --> Abs
@enduml
```

*Figure 1 — Components and layers*

Layers follow the [coding standards](../../../.claude/rules/coding-standards.md): contracts in Abstractions, the resolver, dispatcher and in-memory scheduler in `OoBDev.ComplexEvents` (host-free), the EF store in its own Framework project (EF Core Relational only, like `OoBDev.System.Text.Templating.EntityFrameworkCore`), and the Event Hubs adapter in ExternalServices referencing Abstractions only. Registration is `TryAddComplexEvents()` and `TryAddEntityFrameworkSchedules()` in each project's `ServiceCollectionExtensions`.

## API sketch

```csharp
public interface IComplexEventHandler<in TEvent> { Task HandleAsync(TEvent @event, CancellationToken cancellationToken); }

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ComplexEventHandlerAttribute(Type eventType) : Attribute { public int Order { get; init; } }

public interface IComplexEventDispatcher { Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken); }

public interface IScheduleStore
{
    Task<ScheduleItem> RegisterAsync(string key, DateTimeOffset dueAt, string eventType, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken);
    Task<IReadOnlyList<ScheduleItem>> LeaseDueAsync(DateTimeOffset now, int max, TimeSpan lease, CancellationToken cancellationToken);
    Task CompleteAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ReleaseAsync(string key, CancellationToken cancellationToken);
}
```

The scheduler is a hosted service that leases due items, dispatches them and completes them. It reads `TimeProvider`, never the system clock. A recurring schedule is a handler that registers its next item.

## Scheduling rules

- `RegisterAsync` with an existing key updates the due time (re-registration, not a duplicate).
- A lease that expires without `CompleteAsync` makes the item due again, so handlers must be idempotent; the dispatcher passes the item id as the idempotency key.
- A handler failure leaves the lease to expire and counts an attempt; after the configured maximum the item moves to a failed state and is logged, not retried silently.

## Testing strategy

**Table 2 — Test plan**

| Area | Tier | What |
|------|------|------|
| Resolver and dispatcher | Unit | order, multiple handlers, aggregate failure, no match |
| Scheduler with a fake `TimeProvider` | Unit | runs once at the due time; re-registration; failure attempts |
| EF store | Simulate | SQLite in memory: lease exclusivity with two stores, expiry, completion |
| EF store on SQL Server and PostgreSQL | Integration | `oobd-test-sqlserver` and the PostgreSQL service, same cases |
| Event Hubs adapter | Unit with a fake `IEventHubClient`; Azure emulator Integration if one is added; live as `LiveIntegration` | publish and consume |

*Table 2 maps each area to its tier; coverage target 80% or more for the Framework projects.*

## Open points

- Event Hubs has no emulator in the test stack; add one only when a consumer needs the adapter (`add-integration-service` skill).
- Whether the dispatcher should publish to the message queue on failure (dead letter) is left to the first consumer.
