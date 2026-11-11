# CQRS and Event Sourcing — API Design

[← Architecture](architecture.md) · [↑ Overview](README.md) · [Testing strategy →](testing-strategy.md)

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
| `IEventStore` | `AppendAsync(streamId, expectedSequence, events)` and `ReadAsync(streamId, fromSequence)`; throws or returns a conflict result on sequence mismatch | R1, R3 |
| `EventEnvelope` | Stream id, sequence, type name, timestamp, payload | R1 |
| `AggregateRoot` | Applies events to change state, collects uncommitted events, rebuilds from history | R2 |
| `IAggregateRepository<T>` | Loads an aggregate by replay and saves its uncommitted events through `IEventStore` | R2, R3 |
| `IProjection` | `ApplyAsync(EventEnvelope)` updates one read model | R5 |
| `IProjectionCheckpointStore` | Reads and writes the last applied sequence per projection | R6 |
| `IEventUpcaster` | Converts an older event payload to the current shape on read | R7 |

Commands and queries use the existing `IHandler<TRequest, TResponse>`; no new handler contract is added (R4). Time uses `TimeProvider` (R8).

## Registration

Following the provider/factory pattern, `AddEventSourcing()` would register defaults with `TryAdd*` and select the store by configuration, as the other keyed providers do. An in-memory store is the default for tests and samples.

[Testing strategy →](testing-strategy.md)
