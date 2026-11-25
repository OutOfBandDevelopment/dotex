# EF migration sets per database: migration plan

**Status:** done, and superseded in direction by the [final-state plan](../DesiredStateDatabase/final-state-plan.md) (owner, 2026-11-24; confirmed 2026-11-25). Confirmed by the owner 2026-11-20 (Q27, option A). Built 2026-11-20: `OoBDev.Data.Migrations.Postgres` and `.Sqlite` with the right audit, suppressions and schedule sets, and the SQL Server project `OoBDev.Data.Schema.SqlServer` (dacpac, kept equal to the models by a Unit test). The example host stays on `EnsureCreated` (SQL Server, no EF migrations by design); the event schema registry is built too.
**Decision (owner Q20, 2026-11-20):** one migrations project per database holding the sets of all the stores, one folder and one `MigrationsHistoryTable` per context; the event store keeps its provider projects; start with the right audit store. Layering rule: [layering-and-migration-plans](../../../.claude/rules/layering-and-migration-plans.md).

[Architecture](architecture.md) · [Testing strategy](testing-strategy.md)

## Contents

- [Projects](#projects)
- [Contexts and history tables](#contexts-and-history-tables)
- [Reference changes](#reference-changes)
- [Order of work](#order-of-work)
- [Verification](#verification)

## Projects

**Table 1 — Project moves and additions**

| Project | Current location | Target location (layer) | Namespace | Package or deployment unit | Notes |
| --- | --- | --- | --- | --- | --- |
| `OoBDev.Data.Migrations.Postgres` | does not exist | `src/Framework` (host-free, provider package) | `OoBDev.Data.Migrations.Postgres` | NuGet package, same unit as the event store provider | Npgsql provider, one `Migrations/<Context>/` folder per context |
| `OoBDev.Data.Migrations.Sqlite` | does not exist | `src/Framework` | `OoBDev.Data.Migrations.Sqlite` | NuGet package | Used by tests and small hosts |
| SQL Server dacpac project | does not exist | `src/Framework` (SQL project, built with `OoBDev.DacFx`) | n/a (SQL) | dacpac artifact, not a NuGet package | Desired-state tooling is under evaluation ([DesiredStateDatabase](../DesiredStateDatabase/)); the dacpac covers the same tables |
| `OoBDev.EventSourcing.Postgres` | `src/Framework` | unchanged | unchanged | unchanged | Keeps its own `Migrations` (the event store contract is separate) |

## Contexts and history tables

**Table 2 — Contexts covered, first to last**

| Order | Context | Defined in | Table | History table |
| --- | --- | --- | --- | --- |
| 1 | `RightAuditDbContext` | `OoBDev.AspNetCore.EntityFrameworkCore` | `RightAudit` | `__EFMigrationsHistory_RightAudit` |
| 2 | `SuppressionsDbContext` | `OoBDev.Communications.EntityFrameworkCore` | `Suppressions` | `__EFMigrationsHistory_Suppressions` |
| 3 | `ScheduleDbContext` | `OoBDev.ComplexEvents.EntityFrameworkCore` | `ComplexEventSchedule` | `__EFMigrationsHistory_Schedule` |
| 4 | `LoadedRowsDbContext` | `OoBDev.DataLoader.EntityFrameworkCore` | caller-defined | out of scope: the rows table is the caller's |

Messaging and template stores named in the backlog have no `DbContext` in the tree today; they join when they exist.

## Reference changes

- The migrations projects reference the context-owning projects (never the reverse), so a host that wants a database references one provider package and calls `Database.MigrateAsync()` or applies the scripts.
- `UseNpgsql`/`UseSqlite` calls pass `MigrationsAssembly` and `MigrationsHistoryTable` per context through a small `Use<Context>Migrations` extension in each provider project.
- The example host (`OoBDev.Example.WebApi`) replaces `EnsureCreated` for the right audit store (if used) with the SQL Server dacpac or `MigrateAsync` on the provider chosen by configuration.

## Order of work

1. Right audit store: Postgres and Sqlite sets, tests against the Postgres container and in-memory Sqlite.
2. Suppressions, then schedule.
3. SQL Server dacpac project for the same tables.
4. Event payload schema registry (built 2026-11-20, see [schema-registry](schema-registry.md)).

## Verification

Each set is applied to an empty database in a test (Sqlite in process, Postgres in the container), then `dotnet ef migrations has-pending-model-changes` equivalent is asserted so the snapshot matches the model.
