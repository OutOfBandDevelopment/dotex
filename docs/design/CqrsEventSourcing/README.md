# CQRS and Event Sourcing Reference Design — Overview

**Status:** Draft · **Date:** 2026-11-11 · **Related:** [extraction table](../../migration/harvest-d7-comparison.md#table-2) · [harvest candidates](../../harvest-candidates.md) · [design document standard](../../patterns-discovery/06-design-document-standard.md)

A reference design for building command/query separation and event-sourced aggregates on the OoBDev framework. It is a teaching baseline plus an infrastructure target, not a port: nothing is copied from the pattern references, and no library is built until a consumer needs one.

## Contents

1. [Requirements](requirements.md) — goals, non-goals, numbered requirements
2. [Architecture](architecture.md) — write side, read side, how it relates to the SharedFramework "Complex Events" epic
3. [API design](api-design.md) — the proposed contracts
4. [Testing strategy](testing-strategy.md) — requirement to test map

### List of Figures

1. [Figure 1 — Write and read sides](architecture.md#figure-1)

### List of Tables

1. [Table 1 — Requirements](requirements.md#table-1)
2. [Table 2 — Sources and what they teach](architecture.md#table-2)
3. [Table 3 — Relation to Complex Events](architecture.md#table-3)
4. [Table 4 — Proposed contracts](api-design.md#table-4)
5. [Table 5 — Requirement coverage](testing-strategy.md#table-5)

## Summary

Commands change aggregates, aggregates raise events, an append-only store keeps the events, and projections build read models from the event stream. The pattern explanations come from `fohjin-cqrs` (read-only, linked not copied) and the store design from `event-sourcing`; the ordering primitive comes from the planned hybrid logical clock. Contracts follow the framework patterns: handler `IHandler<TRequest, TResponse>`, provider and factory, `TryAdd*` registration, `TimeProvider` for time.

[Requirements →](requirements.md)

## Decisions

Confirmed by the owner on 2026-11-11 (the recommended options):

1. **Event envelope:** CloudEvents 1.0 compatible, with an `OoBDev` extension for stream id and sequence.
2. **Store contract:** append with an expected sequence (optimistic concurrency), read by stream, follow by global position.
3. **Home:** Framework (`OoBDev.EventSourcing` plus Abstractions); SQL and message-queue stores are adapters.
4. **First cut:** minimal. Snapshots and upcasting are separate backlog items.
5. **Erasure:** crypto-shredding per subject is the supported route; no event deletion and no redaction events.

The library build is now unblocked and sits in `BACKLOG.md`.
