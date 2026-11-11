# Feature request: Event, document and history store abstractions with an embedded SQLite provider

- **Status:** triaged
- **Requested by:** owner (filed from the notes repository; origin is the `ctrl-home` proposal, `docs/proposal/02-architecture.md` and `11-build-on-dotex.md`)
- **Date:** 2026-11-10
- **Area:** new, near `OoBDev.MessageQueueing`, `OoBDev.Caching` and `OoBDev.ComplexEvents`

## Need

`ctrl-home` is planned as CQRS and event sourcing on an embedded store. Its domain depends on `IEventStore` (append with expected version, read a stream, subscribe),
`IMessageQueue` (durable, with ack and redelivery), `IDocumentStore` (read models, configuration) and `IHistoryStore` (tiered time-series rollups). Its own proposal says it
saw no dotex package for event sourcing, SQLite stores or a scheduler, and that generic infrastructure is a candidate for dotex first. Other notes projects that keep
state on a small device (the resume event-sourcing slice, the telemetry research) would use the same pieces.

## Proposed behavior

1. **Abstractions** in a framework package: an event store with optimistic concurrency, a durable queue with an outbox, a document store with JSON documents and
   a history store with rollup tiers. No dependency on a particular database.
2. **Embedded provider** on SQLite in WAL mode (EF Core, or `Microsoft.Data.Sqlite` behind the same interfaces if EF Core is too heavy for small boards).
3. **Backup hooks:** a consistent snapshot (online backup API or `VACUUM INTO`) and sealed event segments, so a host can back up while running.
4. **Test double:** an in-memory implementation for unit tests.

## Acceptance criteria

- Given a stream at version N, when two writers append with expected version N, then exactly one succeeds and the other gets a concurrency error.
- Given a subscriber that stops before acknowledging, when it restarts, then the message is delivered again.
- Given a running host, when a snapshot is taken, then it restores to a consistent state with no partial transaction.
- Given the same test suite, when run against the in-memory and SQLite providers, then both pass.

## Existing code checked

- A search of `docs/features/request` and `src` for event sourcing, `IEventStore`, `IDocumentStore` and `IHistoryStore` found nothing. SQLite appears only in the
  desired-state database request (schema tooling, not a store).
- `OoBDev.MessageQueueing`, `OoBDev.Caching` and `OoBDev.ComplexEvents` may overlap on the queue and subscription parts. Not read; a maintainer will know.

## Security and operations

Local files on the host; place, permissions and encryption at rest are the host's choice. Single-writer SQLite is a limit that `ctrl-home` spike 2 is meant to measure.

## Priority

- **Impact:** removes the largest piece of new code `ctrl-home` would otherwise write, and gives other small-device projects the same base.
- **Needed by:** no date
- **Workaround today:** `ctrl-home` writes its own.

## Open questions

- Does this belong inside `OoBDev.MessageQueueing` and `OoBDev.ComplexEvents`, or as a separate package?
- LiteDB or an LMDB-style store as a second provider (`ctrl-home` spike 2 compares them)?
- Is the history store in scope here, or a separate request tied to OpenTelemetry export?

## Triage notes

Origin: notes repository `projects/dotex-consolidation/README.md` (mapping section). Maintainers: accept, schedule or decline.

2026-11-11: triaged. Accepted as a backlog idea: design first (`docs/patterns-discovery/06-design-document-standard.md`), no code until a consumer appears. Backlog: BACKLOG.md, Consolidation requests.
