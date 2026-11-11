# CQRS and Event Sourcing — Testing Strategy

[← API design](api-design.md) · [↑ Overview](README.md)

**Status:** Draft · **Date:** 2026-11-11

## Contents

- [Approach](#approach)
- [Coverage](#coverage)

### List of Tables

1. [Table 5 — Requirement coverage](#table-5)

## Approach

When a library is built it sits in the Framework layer, so Unit tests target 80% or more coverage with an in-memory store and a fake `TimeProvider`. A database-backed store is tested in the Integration category against the Docker stack. Nothing here is built yet; the table fixes what the tests must show.

## Coverage

<a id="table-5"></a>**Table 5 — Requirement coverage**

| Requirement | Test |
|-------------|------|
| R1 | Appended events read back in order with their sequence and type |
| R2 | An aggregate rebuilt from history equals the one that produced it |
| R3 | Appending with a stale expected sequence fails and stores nothing |
| R4 | A command handler is resolved from DI and changes state only through events |
| R5 | A projection rebuilt from sequence zero equals the incrementally built one |
| R6 | A restarted projection resumes from its checkpoint without reapplying events |
| R7 | An old payload is upcast on read and the stored bytes are unchanged |
| R8 | With a fake `TimeProvider` event timestamps are deterministic |
| R9 | Not tested until an extension is built |
| R10 | Documentation check only: the relation table exists |

## Entity Framework stores

One shared contract test class runs against every database provider: append and read, stale-sequence conflict, global feed order, entity version conflict and checkpoint round trip.

- **SQLite** (in-memory connection): `Simulate`, runs in the normal test run.
- **SQL Server**: `Integration`, on the existing SQL Server container.
- **PostgreSQL**: `Integration`, needs a PostgreSQL service added to the test stack (`add-integration-service` skill) with `POSTGRES_*` parameters in `src/.runsettings` and `TEST_VARIABLES.md`.
- A concurrency test appends to one stream from several tasks and expects exactly one winner per sequence on each database.
- **SQL Server schema parity:** deploy `OoBDev.EventSourcing.SqlServer.Database` (the dacpac) to a throwaway database and compare tables, columns, types and indexes with the EF model; `Integration`.
