# Feature request: Desired-state database tooling

- **Status:** new
- **Requested by:** owner (filed from the notes repository, `projects/dotex-consolidation/desired-state-database-tooling.md` and `sqldef-evaluation/`)
- **Date:** 2026-10-28
- **Area:** `OoBDev.DacFx`, `OoBDev.DacPacCompiler.Cli`, `OoBDev.ComplexEvents.DatabaseExtensions` (nearest existing areas); new area unknown

## Need

Derive migrations from plain `CREATE` scripts for Postgres, SQL Server, SQLite and Oracle with free, MIT-compatible tooling, and also manage what a schema diff cannot model:
extensions, queues and event streams. dotex has a dacpac builder and validator for SQL Server, but nothing for the other three databases. The owner expects past tools to have
failed in ways not described to me.

## Proposed behavior

Three layers (from the notes, not confirmed against current docs):

1. Schema diff, delegated to existing tools (sqldef, DacFx, pg-schema-diff) behind one provider interface; Oracle needs its own adapter or hand-written scripts.
2. Idempotent, ordered "ensure" scripts per provider for what the diff cannot see, for example `001_extensions.sql` and `020_queues.sql`.
3. Optional feature manifest (`queue: orders`, `extension: pgmq`) that compiles to the layer 2 scripts.

First step in the notes: a short bake-off of sqldef, pgschema and Atlas on a real schema (`sqldef-evaluation/01-test-plan.md`), then build only what is missing.

## Acceptance criteria

- Given a folder of `CREATE` scripts and an empty database, when the tool runs, then the database matches the scripts, for each supported engine.
- Given a changed script, when the tool runs, then it applies only the difference and refuses unsafe changes without an explicit flag.
- Given an ensure script, when run twice, then the second run changes nothing.
- Given the bake-off, when scored against the test plan, then it records a buy or build decision per engine with the date and tool versions.

## Existing code checked

- `OoBDev.DacFx` and `OoBDev.DacPacCompiler.Cli` cover SQL Server dacpacs. `OoBDev.ComplexEvents.DatabaseExtensions` has post-deployment scripts. I did not check how they would plug in.
- `docs/harvest-candidates.md` lists `postsql-playground` (PgProj, a desired-state PostgreSQL experiment) as relevant; it was not run.
- No dotex doc mentions sqldef or desired-state tooling beyond the harvest list and the suggested-names note.
- The tool survey in the notes was written from memory; licences and paid tiers are unverified.

## Security and operations

Runs DDL against databases, so it needs credentials with schema rights and a plan-then-apply step with hazard gates (data loss, locks). Oracle and SQL Server need licences or
containers for testing. The cost is mostly test infrastructure: containers for Postgres 14 to 18, SQL Server, and Oracle (availability unknown).

## Priority

- **Impact:** nothing is blocked.
- **Needed by:** no date.
- **Workaround today:** dacpacs for SQL Server; hand-written migrations elsewhere.

## Open questions

- Schema only, or also functions, triggers and views?
- Wrap existing diff engines, or write a custom one?
- Queues and streams: portable tables first, or native objects per database?
- Oracle: custom catalog-diff adapter, or hand-written repeatable scripts?
- Is .NET the stack? The owner has not answered.
- Do the sqldef evaluation and this effort merge? The notes leave it open.

## Triage notes

Maintainers fill this in.
