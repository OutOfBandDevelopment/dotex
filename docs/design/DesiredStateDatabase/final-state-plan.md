# Desired-State Database Tooling — Final-state plan (replace EF migrations)

[← Overview](README.md)

**Status:** proposed 2026-11-24, confirmed by the owner 2026-11-25 (Q37 option A: retire all the EF migration projects, data changes are hand-written `*.once.sql` ensure scripts, EF hook stays in `OoBDev.Data.DesiredState`). Removal is next (plan phase E). Decision it follows: owner, 2026-11-24, "I would prefer it to be final states/desired state versus migration based" ([owner decisions](../../../.claude/rules/owner-decisions.md)). Layering rule: [layering-and-migration-plans](../../../.claude/rules/layering-and-migration-plans.md), so the table below is confirmed before any project moves.

## Contents

- [Where we are](#where-we-are)
- [Target](#target)
- [Projects](#projects)
- [What a final state cannot do](#what-a-final-state-cannot-do)
- [Order of work](#order-of-work)
- [Verification](#verification)

## Where we are

- **SQL Server** is already final-state: `OoBDev.Data.Schema.SqlServer` holds `Tables/*.sql` generated from each `DbContext` (`GenerateCreateScript()`), and `SqlServerSchemaTests` fails when a file differs from the model.
- **PostgreSQL and SQLite** are migration-based: `OoBDev.Data.Migrations.Postgres` and `.Sqlite` hold generated EF migrations (`Initial*`, `AddLastRun`) with snapshots and one history table per context ([older plan](../CqrsEventSourcing/migrations-plan.md)). The event store providers `OoBDev.EventSourcing.Postgres` and `.Sqlite` carry their own `InitialCreate` migrations.
- **The tool is built:** `OoBDev.Data.DesiredState` wraps sqldef for PostgreSQL, SQL Server and SQLite with the drop gate and the type-change check, plus the ensure-script runner and the `desired-state` CLI.

## Target

One final schema per store and engine, as plain `CREATE` scripts, diffed and applied by `OoBDev.Data.DesiredState`. The scripts are generated from the model (as SQL Server's already are) and a Unit test keeps them equal to it, so the model stays the single source and the script is what gets reviewed. A host brings a database to the final state with the provider (`--check` for drift, plan, then apply with the gates) instead of `Database.MigrateAsync()`. No `dotnet ef migrations add`; a model change regenerates the script (`REGENERATE_SQL_SCHEMA=1`).

## Projects

**Table 1 — Project moves and removals** (for confirmation)

| Project | Current location | Target | Namespace | Deployment unit | References that change |
| --- | --- | --- | --- | --- | --- |
| `OoBDev.Data.Schema.SqlServer` | `src/Framework` | unchanged | n/a (SQL) | dacpac | none |
| Final-state scripts for PostgreSQL and SQLite | do not exist | new folders `Tables/postgres` and `Tables/sqlite` beside the SQL Server ones, in one project renamed `OoBDev.Data.Schema` (the SQL Server folder keeps the dacpac build) | n/a (SQL) | scripts as embedded or copied files; dacpac for SQL Server | none until hosts adopt it |
| `OoBDev.Data.Migrations.Postgres` and `.Sqlite` | `src/Framework` | removed after the scripts and tests exist | `OoBDev.Data.Migrations.*` | NuGet packages (unreleased) | hosts and tests that call `Use*Migrations` or `MigrateAsync` move to the provider |
| `OoBDev.Data.Migrations.Tests` | `src/Framework` | renamed `OoBDev.Data.Schema.Tests`; keeps the model-equals-script tests, loses the migration-applies tests | `OoBDev.Data.Schema.Tests` | test project | solution file |
| `OoBDev.EventSourcing.Postgres` and `.Sqlite` migrations | `src/Framework` | their `Migrations/` folders replaced by a final-state script; the event store contract is unchanged | unchanged | unchanged | the EventSourcing EF tests that run `MigrateAsync` |
| `OoBDev.Data.DesiredState` | `src/Framework` | unchanged | unchanged | NuGet package | gains an engine-aware script folder helper only if the tests need it |

The package is unreleased, so removal needs no forwarders ([breaking changes](../../../.claude/memory/feedback-breaking-changes-ok.md)).

## What a final state cannot do

- **Data changes:** backfills, splitting a column, moving rows. A diff has no data steps. The ensure scripts (`*.once.sql`, recorded by name and hash) already run a one-off script once; they are the place for data changes, written by hand.
- **Renames:** sqldef plans a drop and add unless the script carries the `@renamed` annotation, so a rename is an annotated line in the final script for one release.
- **Type narrowing:** no candidate names the hazard; the wrapper's type-change gate refuses until `AllowTypeChange` is set.
- **History:** there is no list of past changes; `git log` of the script is the history.

## Order of work

1. Generate and test the PostgreSQL and SQLite scripts for the right audit, suppressions and schedule stores (Unit test equal to the model, like SQL Server).
2. Apply them through `DesiredState` in the existing Integration (Postgres container) and Simulate (SQLite in memory) tests, replacing the migration-applies tests.
3. Switch the example host and the `Use*Migrations` call sites to the provider.
4. The event store providers, then remove the migrations projects and update their readmes, the older plan (marked superseded) and `docs/changes/`.

## Verification

Each store: script equals model (Unit); empty database to final state, then a second run plans nothing (Simulate for SQLite, Integration for PostgreSQL); a model change regenerates the script and the plan shows exactly that change; the gates refuse a drop and a narrowing without their flags. Run results are stated plainly in the change document.
