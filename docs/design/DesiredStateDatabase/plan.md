# Desired-State Database Tooling — Build plan for PostgreSQL and SQLite

[← Overview](README.md)

**Status:** proposed 2026-11-24; phases A to D built and tested 2026-11-24, phase E confirmed 2026-11-25 (Q37 A), G skipped (Q39: sqldef stays; the binary is fetched by `SqldefInstaller`, built 2026-11-25), F is not needed yet (see [Starting point](#starting-point)). Owner decisions it follows: final states instead of migrations (2026-11-24, [owner decisions](../../../.claude/rules/owner-decisions.md)), wrap sqldef (Q34 A), Oracle and Atlas rejected (2026-11-24). Phase E removes projects, so it waits for Q37; phase G creates a project, so it waits for a confirmed migration plan (phase F adds no project: the canonical model is internal only, owner, 2026-11-24) ([layering rule](../../../.claude/rules/layering-and-migration-plans.md)).

## Contents

- [Goal](#goal)
- [Starting point](#starting-point)
- [Phases](#phases)
- [Canonical model rules](#canonical-model-rules)
- [Engine differences that break a shared plan](#engine-differences-that-break-a-shared-plan)
- [Objects that can only be dropped and created](#objects-that-can-only-be-dropped-and-created)
- [SQL Server](#sql-server)
- [Open choices](#open-choices)
- [Verification](#verification)

### List of Tables

1. [Table 1 — Starting point](#table-1)
2. [Table 2 — Phases](#table-2)
3. [Table 3 — Engine differences](#table-3)
4. [Table 4 — Drop and create only](#table-4)

## Goal

An EF Core `DbContext` on PostgreSQL or SQLite reaches its final schema through `OoBDev.Data.DesiredState` (plan, gates, apply) instead of `MigrateAsync()`, with data changes in once-scripts, and with a package format, a refactor log and drift detection that give the behaviour of a dacpac. SQL Server keeps its dacpac route behind the same provider interface.

## Starting point

<a id="table-1"></a>
**Table 1 — Starting point**

| Piece | State |
| --- | --- |
| `IDesiredStateProvider`, `SqldefProvider`, `PlanAnalyzer`, `ApplyOptions`, `EnsureScriptRunner`, `ManifestCompiler`, `AdoScriptExecutor`, `desired-state` CLI | Built (2026-11-23), 40 Unit tests pass |
| psqldef on Postgres (Integration) and sqlite3def (spike replay, 8 `DevLocal` tests) | Passed |
| SQL Server final-state scripts (`OoBDev.Data.Schema.SqlServer`, `GenerateCreateScript()`, script-equals-model test) | Built |
| PostgreSQL and SQLite final-state script for Suppressions (`OoBDev.Data.Schema.Tests/Schema/{postgres,sqlite}`) | Built 2026-11-24 (phase A) |
| Host hook (`EfDesiredStateDeployer`, phase B) | Built 2026-11-24 |
| Other stores (RightAudit, Schedule, event store) | Built 2026-11-24 (phase C) |
| Package, refactor log | Not built |
| Whether sqldef parses EF Core's generated PostgreSQL and SQLite scripts | **Yes** (phase A, 2026-11-24): empty database to final state is 3 statements, the second plan 0, on Postgres 17 (container) and SQLite |

## Phases

<a id="table-2"></a>
**Table 2 — Phases**

| Phase | Content | Done when | Needs |
| --- | --- | --- | --- |
| A | **EF bridge slice (built 2026-11-24)** for one small store (Suppressions): design-time factory per provider, `GenerateCreateScript()` for PostgreSQL and SQLite, a Unit test that the committed script equals the model (`REGENERATE_SQL_SCHEMA=1` regenerates), then apply through sqldef and plan again | Second plan has 0 statements on Postgres (Integration) and SQLite (Simulate); a go or no-go on sqldef parsing EF output is recorded | none |
| B | **Host hook (built 2026-11-24, `EfDesiredStateDeployer`; no production call site used `MigrateAsync`, tests move in phase E)** `EnsureDesiredStateAsync(DbContext)` replacing `MigrateAsync()`: generate, plan, gate, apply; seed data (`HasData`) moved to `*.once.sql` | Example host and the `Use*Migrations` call sites use it; tests cover the gates | A go |
| C | **Remaining stores (built 2026-11-24: RightAudit, Schedule in `OoBDev.Data.Schema.Tests/Schema`, event store in `OoBDev.EventSourcing.EntityFrameworkCore.Tests/Schema`)** (audit, right, schedule, then the event store providers) generated and tested like phase A | Every store has script-equals-model and empty-to-final tests | B |
| D | **Package and refactor log (built 2026-11-24: `SchemaPackage`, `SchemaPackageFormat`, `SchemaPackageDeployer`, `AdoAppliedScriptStore`; refactor entries and the package hash share one `__desired_state_history` table instead of two; no publish profile, the `ApplyOptions` argument serves that purpose; CLI verbs `pack` and `deploy` on SQLite, PostgreSQL and SQL Server):** zip with `manifest.json`, ordered `schema/*.sql`, `predeploy/` and `postdeploy/` scripts, `refactorlog.json`, optional publish profile; unapplied refactor entries run first (for example `RENAME COLUMN`), are recorded in a `__refactorlog` table, then sqldef plans; deployment history table with the package hash | Rename scenario S4 passes without data loss; a repeat deploy applies nothing | C |
| E | **Built 2026-11-25** (Migrations projects and event store `Migrations/` folders removed, tests project renamed, event store tests use the desired-state route) **Remove** `OoBDev.Data.Migrations.Postgres`, `.Sqlite`, the event store `Migrations/` folders; rename the tests project `OoBDev.Data.Schema.Tests`; update readmes, mark the older plan superseded | Build and Unit tests green without them | Q37 answered, C |
| F | **Not needed so far (phases A to D built without it; build when a gap appears)** **Internal canonical model (owner, 2026-11-24: internal only, to reduce code):** `internal` types in `OoBDev.Data.DesiredState` (a small dialect with type map, quoting and capability flags, only as far as phases A to D need it); no public interface, no package surface, no separate lint project. The rules below are a checklist in this document, enforced only by the existing script-equals-model and second-plan-is-empty tests | Same model applied on SQLite, PostgreSQL and SQL Server in tests | none (no new project) |
| G | **Native differ spike** (`src/Spikes`): `ISchemaInspector` per engine, typed plan steps, `TableRebuildStep` for SQLite, scenarios S1 to S12 as tests; decide against what sqldef plus phase D already covers | Written result and a go or no-go | confirmed migration plan |

Phases A to D need no new project. Phase D lives in `OoBDev.Data.DesiredState`. Optional: a tmpfs data directory for `postgres-dev` in `containers/bakeoff` to speed the scratch database (the saving is not measured).

## Canonical model rules

The EF model is the single source; per-engine scripts are generated from it and kept equal by tests. The canonical model stays internal (owner, 2026-11-24, to reduce code): these rules are guidance for writing entity configurations, not a public contract or a generated lint. A portable subset keeps one model valid on all three engines:

- names at most 63 bytes, quoted consistently, no two names that differ only by case;
- a maximum length on every string, with a `CHECK` where SQLite must enforce it;
- integers for money and time (ticks), never `decimal`;
- keys generated by the application (`Guid`, `long`), not identity columns;
- text keys stored normalised (lower case) so collation cannot matter;
- no clustered or included-column indexes in the shared model;
- functions, procedures, extensions and online index options live in per-engine ensure scripts, outside the shared model.

## Engine differences that break a shared plan

Facts below are from the repository's own `Suppressions` table (SQL Server `nvarchar(320)`, PostgreSQL `character varying(320)`, SQLite `TEXT`) and from knowledge of the engines; the rest was not tested here.

<a id="table-3"></a>
**Table 3 — Engine differences**

| Area | SQL Server | PostgreSQL | SQLite | Effect |
| --- | --- | --- | --- | --- |
| Length limits | enforced | enforced | `TEXT(320)` not enforced | same schema, different rules |
| Decimals | exact | exact | no exact type | use integer minor units |
| Text comparison | case-insensitive default | case-sensitive | case-sensitive | unique keys hold different row sets |
| Name length | 128 | 63 bytes, truncated | unlimited | long EF constraint names collide |
| Default constraints | named objects | inline | inline | must know the name before dropping a column |
| Change a column type | `ALTER COLUMN` | `ALTER ... TYPE ... USING` | table rebuild | one canonical step, three plans |
| Rename column | `sp_rename` | `RENAME COLUMN` | `RENAME COLUMN` (3.25+) | |
| Foreign keys | always enforced | always enforced | off unless `PRAGMA foreign_keys` per connection | deployed store may not enforce them |
| Schemas | yes | yes | none | schema name meaningless in SQLite |
| DDL in a transaction | yes | yes (not `CONCURRENTLY`) | yes, but the pragma cannot change inside one | plan transaction boundaries differ |
| Script batches | `GO` separator | `;` | `;` | splitter must handle `GO` |
| Scratch database | server or container | server or container | in memory | cost of a build step |

## Objects that can only be dropped and created

<a id="table-4"></a>
**Table 4 — Drop and create only**

| Object | SQL Server | PostgreSQL | SQLite |
| --- | --- | --- | --- |
| View | `CREATE OR ALTER` | `CREATE OR REPLACE` if columns keep names and types and new ones go last, else drop and create | drop and create |
| Materialized view | indexed view where allowed | drop and create | none |
| Index | drop and create (or `DROP_EXISTING = ON`) | drop and create | drop and create |
| Check, unique, foreign key | drop and add | drop and add (`NOT VALID` then `VALIDATE`) | table rebuild |
| Trigger | `CREATE OR ALTER` | `CREATE OR REPLACE` (14+) plus the function | drop and create |
| Function, procedure | `CREATE OR ALTER` | `CREATE OR REPLACE`; drop and create when the return type or parameters change | none |
| Enum type | n/a | add a value only | none |

The planner needs dependency order (a drop can be blocked by dependents) and a fallback from replace to drop and create.

## SQL Server

Same provider interface and options, no second differ: a `SqlServerDacFxProvider` maps the canonical options to `DacDeployOptions`, the refactor log to a DacFx `.refactorlog` and the pre and post scripts to the dacpac's own, using the existing `OoBDev.DacFx` wrappers. A native T-SQL comparer would cost more than PostgreSQL and SQLite together for little gain over DacFx. Revisit only if DacFx is dropped or per-object policy must be identical on all three engines.

## Open choices

| Choice | Recommendation | Where it is decided |
| --- | --- | --- |
| Remove the EF migrations projects | Yes, after phase C | Confirmed 2026-11-25 (Q37 A) |
| Portable-subset rules as a lint test or as guidance | Decided 2026-11-24: guidance only, canonical model internal, no new lint code | owner |
| Native differ (phase G) or stay on sqldef plus phase D | Stay on sqldef until phase D shows a gap | spike result |

## Verification

Per store: script equals model (Unit); empty database to final state, then a second run plans nothing (Simulate for SQLite, Integration for PostgreSQL); a model change regenerates the script and the plan shows exactly that change; the gates refuse a drop and a narrowing without their flags; the rename scenario keeps its data after phase D. Results are stated plainly in the change document, including what was not run.
