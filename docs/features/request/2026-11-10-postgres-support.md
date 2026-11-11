# Feature request: PostgreSQL support alongside the SQL Server pieces

- **Status:** triaged
- **Requested by:** owner (filed from the notes repository, `projects/postgres-backup-scheduling-spike` and `projects/dotex-consolidation/sqldef-evaluation`)
- **Date:** 2026-11-10
- **Area:** `OoBDev.DacFx` neighbours, data access and the SQL Service Broker tooling

## Need

The notes projects increasingly use Postgres: the NAS storage catalog (EF Core on Postgres), the Postgres backup scheduling and log shipping spike, and the sqldef,
pgschema and Atlas evaluation. dotex's database tooling (dacpac builder and validator, Service Broker tools) is SQL Server only as far as the existing requests show,
and the desired-state request already lists Postgres as a target without a plan for runtime support.

## Proposed behavior

1. A statement of which dotex data packages are database-neutral and which are SQL Server only.
2. A Postgres provider where a neutral abstraction exists (connection handling, health checks, test fixtures against a container).
3. A queue and event-stream answer for Postgres in place of Service Broker (for example `LISTEN`/`NOTIFY` or a table-based outbox), decided after the
   `sqldef` bake-off.

## Acceptance criteria

- Given the data test fixtures, when pointed at a Postgres container, then the neutral tests pass.
- Given the docs, when read, then each data package states its supported databases.

## Existing code checked

- `2026-10-28-desired-state-database-tooling.md` covers schema derivation for Postgres, SQL Server, SQLite and Oracle; this request is about runtime support and fixtures.
- Search of `src` project files for Postgres and Npgsql found nothing. Package README files were not all read.

## Security and operations

Connection strings and credentials stay in configuration; the test fixture uses a throwaway container.

## Priority

- **Impact:** speculative until the schema tooling decision is made.
- **Needed by:** no date
- **Workaround today:** the notes projects use Npgsql and EF Core directly.

## Open questions

- Is Postgres meant to be a first-class target, or only a schema tooling target?
- Should this wait for the `sqldef` evaluation result?

## Triage notes

Origin: notes repository `projects/dotex-consolidation/README.md` (mapping section). Lowest priority of three filed together. Maintainers: accept, schedule or decline.

2026-11-11: triaged. Accepted as a backlog idea: design first (`docs/patterns-discovery/06-design-document-standard.md`), no code until a consumer appears. Backlog: BACKLOG.md, Consolidation requests.
