# Store instances: schema, prefix or file per instance

[← Overview](README.md)

**Status:** design, nothing built yet (2026-11-25). Owner question of 2026-11-25: can the small EF stores be deployed into one database, possibly several times, by configuration? Answer: yes; the plan filter that looked like the hard part is a sqldef setting.

## Contents

- [Goal](#goal)
- [Mapping per engine](#mapping-per-engine)
- [What the code must do](#what-the-code-must-do)
- [Plan scope: checked 2026-11-25](#plan-scope-checked-2026-11-25)
- [Built 2026-11-25](#built-2026-11-25)
- [Open points](#open-points)

**Tables:** [Table 1 — Mapping per engine](#mapping-per-engine)

## Goal

Each small store (Suppressions, RightAudit, Schedule, event store, a ticket store) keeps its own tables and its own deployment, and can be placed beside other stores, or beside a second copy of itself, in one database by changing configuration only. There is no shared "super database".

## Mapping per engine

One option per store: an instance name. The engine decides how it is used.

**Table 1 — Mapping per engine**

| Engine | Instance name becomes | Notes |
| --- | --- | --- |
| PostgreSQL | schema (`ticket."Tickets"`) | index names are per schema, so no prefix is needed |
| SQL Server | schema | same |
| SQLite | table prefix (`ticket_Tickets`), or a separate file per instance (no prefix at all) | index names share one namespace per file, so the prefix covers index, unique and key names too; a file per instance needs no mapping and is the default |

Cross-file queries on SQLite are possible with `ATTACH DATABASE` (per connection, up to 10 files by default, no cross-file foreign keys, atomic commit across files only outside WAL mode), but a feature that needs them probably belongs in one store.

## What the code must do

1. A small options class per store (`SchemaName`, `TablePrefix`), read in `OnModelCreating`; the provider decides which applies (`HasDefaultSchema` or a naming convention).
2. An `IModelCacheKeyFactory` that includes the schema and prefix, because EF caches one model per context type and a second instance would otherwise reuse the first model.
3. The generated final-state script is produced from the configured model at deploy time. The committed script stays the default instance and the script-equals-model test covers it; one extra test covers a non-default instance per engine.
4. `__desired_state_history` takes the same schema or prefix, so two instances do not see each other's package and refactor records.
5. Once-scripts and ensure scripts use `{schema}` and `{prefix}` tokens substituted per instance.
6. The provider passes the plan scope to sqldef (next section).

## Plan scope: checked 2026-11-25

sqldef plans against the whole database, so another instance's tables look like drops. Checked against sqldef 3.11.30:

- `sqlite3def` skips drops unless asked (`-- Skipped: DROP TABLE`), and `--config-inline 'target_tables: a_.*'` removes the other prefix from the plan.
- `psqldef` with `--enable-drop` planned `DROP TABLE "b"."Notes"` for a second schema; `--config-inline 'target_schema: a'` and `target_tables: a\..*` both removed it.
- `mssqldef` has the same `--config` options but was not run here.

So no plan filtering code is needed in the deployer: `SqldefProvider` must pass a config (`target_schema` on PostgreSQL and SQL Server, `target_tables` on SQLite with a prefix).

## Built 2026-11-25

- `DatabaseTarget` carries `Schema` and `TablePrefix` (`ForInstance`); `SqldefProvider` passes `target_schema` (PostgreSQL, SQL Server) or `target_tables` (SQLite with a prefix) as `--config-inline`.
- `SuppressionsInstance.UseSuppressionsInstance(schema, tablePrefix)` in `OoBDev.Communications.EntityFrameworkCore` places the table, and an `IModelCacheKeyFactory` keeps one model per instance. The helper is internal to that project until Q42 is answered.
- EF emits a `DO $EF$ ... CREATE SCHEMA` block for a schema, which sqldef cannot parse: the block is removed from the desired script and the schema is created by an ensure step.
- Tests (`OoBDev.Data.Schema.Tests/StoreInstanceTests.cs`): model per instance (Unit), two prefixes in one SQLite file (Simulate), two schemas in one PostgreSQL database (Integration); each second plan is empty and skips nothing.

## Open points

- Check `mssqldef` with `target_schema`.
- `{schema}`/`{prefix}` tokens in once and ensure scripts (the history table is per instance since 2026-11-25).
- The other stores (RightAudit, Schedule, event store, AspNetCore, ComplexEvents) adopt the shared helper once Q42 is answered.
