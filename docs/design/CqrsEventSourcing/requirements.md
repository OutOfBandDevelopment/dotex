# CQRS and Event Sourcing — Requirements

[↑ Overview](README.md) · [Architecture →](architecture.md)

**Status:** Draft · **Date:** 2026-11-11

## Contents

- [Goals](#goals)
- [Non-goals](#non-goals)
- [Requirements](#requirements-1)

### List of Tables

1. [Table 1 — Requirements](#table-1)

## Goals

- Give the owner a written, worked pattern for event-sourced aggregates and read-model projections that new products can follow.
- Fix the vocabulary and contracts once, so a later Framework library has a settled shape.
- Keep the governing principle: never lose or corrupt data.

## Non-goals

- Porting source from `fohjin-cqrs`, `event-sourcing` or `sync-mesh`. Copying the event-sourcing material waits for the owner (sanitizing first).
- The bank application, UI clients, GraphQL gateway, DPoP, erasure, NATS transport and other product features of those repositories.
- Building a library now. A library is built when a consumer appears.

## Requirements

<a id="table-1"></a>**Table 1 — Requirements**

| Id | Requirement |
|----|-------------|
| R1 | Events are immutable, append-only, and identified by stream id, sequence number and type name. |
| R2 | An aggregate is rebuilt by replaying its events; state changes only by applying events. |
| R3 | Appending uses optimistic concurrency: an append names the expected sequence and fails on a mismatch. |
| R4 | Commands are handled by `IHandler<TRequest, TResponse>` types with injected dependencies, not lambdas. |
| R5 | Read models are projections built from the event stream, rebuildable from sequence zero, and eventually consistent. |
| R6 | A projection records a checkpoint so it resumes after a restart without reprocessing. |
| R7 | Event schema changes are handled by upcasting on read; stored events are never rewritten. |
| R8 | Time comes from `TimeProvider`; ordering across clocks uses the hybrid logical clock once it exists. |
| R9 | Tamper evidence (hash chain) and late-arrival handling are described as optional extensions, not core. |
| R10 | The design states how it relates to the SharedFramework "Complex Events" epic, so the two are not built twice. |

[Architecture →](architecture.md)
