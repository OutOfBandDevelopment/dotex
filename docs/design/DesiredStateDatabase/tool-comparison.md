# Desired-State Database Tooling — Tool comparison

[← Bake-off result](bake-off-result.md) · [↑ Overview](README.md)

**Status:** current as of 2026-11-24. Sources: the [bake-off result](bake-off-result.md) (run 2026-11-20 to 2026-11-23), the `--help` output of the tools built into [`containers/bakeoff`](../../../containers/bakeoff/README.md) on 2026-11-24, and the wrapper code in `OoBDev.Data.DesiredState`. Where a statement was not checked against the tool, it says so.

This report compares the two remaining candidates (sqldef and pgschema; Atlas was dropped by the owner on 2026-11-24) on the points that decide adoption: what each one does, how it is configured, how it is deployed, how it behaves in the scenarios, and what it costs to own. The decision is already made (Q34 A: wrap sqldef, with a drop gate and a type-change check added by the wrapper); this report records why and what would change it.

## Contents

- [Summary](#summary)
- [Identity and scope](#identity-and-scope)
- [Configuration](#configuration)
- [Deployment](#deployment)
- [Behaviour in the scenarios](#behaviour-in-the-scenarios)
- [Safety](#safety)
- [Integration from .NET](#integration-from-net)
- [Pros and cons](#pros-and-cons)
- [Licence and cost](#licence-and-cost)
- [Risks and unknowns](#risks-and-unknowns)
- [What would change the decision](#what-would-change-the-decision)

### List of Tables

1. [Table 1 — Identity and scope](#table-1)
2. [Table 2 — Configuration](#table-2)
3. [Table 3 — Deployment](#table-3)
4. [Table 4 — Scenario results](#table-4)
5. [Table 5 — Safety defaults](#table-5)
6. [Table 6 — Pros and cons](#table-6)
7. [Table 7 — Licence and cost](#table-7)

## Summary

- **sqldef** is the safest default and covers three of our four engines with one family of binaries and one command shape. Its plan is plain SQL text; it never drops without `--enable-drop`. It does not report the hazard of a type change.
- **pgschema** has the best plan format (JSON, a saved plan applied later) but only supports PostgreSQL, plans drops without a flag, and fails on scripts that are not already in dependency order.
- **Atlas** was dropped (owner, 2026-11-24). Its run is kept in the [result](bake-off-result.md#atlas-on-postgresql) as history: the free binary refused views and extensions without a vendor login, which broke REQ-008.
- **Oracle is out of scope** (rejected by the owner, 2026-11-24; no candidate supports it anyway), and none reports the hazard of narrowing a type (S6), so the wrapper's `PlanAnalyzer` is needed whichever tool is wrapped.

## Identity and scope

<a id="table-1"></a>
**Table 1 — Identity and scope**

| | sqldef | pgschema |
|---|---|---|
| Version tested | 3.11.30 | 1.14.1 |
| Language and build | Go; built here from the pinned tag | Go; built here from the pinned tag |
| Engines | PostgreSQL (`psqldef`), SQL Server (`mssqldef`), SQLite (`sqlite3def`), MySQL (`mysqldef`, not tested) | PostgreSQL only |
| Desired state is | Plain `CREATE` statements in SQL | Plain `CREATE` statements in SQL |
| Needs a second database to plan | No | Optional: an embedded Postgres by default, or `--plan-host` |
| Fits our model (plain `CREATE` scripts) | Yes | Yes |

## Configuration

<a id="table-2"></a>
**Table 2 — Configuration**

| | sqldef | pgschema |
|---|---|---|
| Connection | Flags `-h -p -U` plus the database name; `PGPASSWORD` honoured by `psqldef` | Flags `--host --port --user --db`, or `PGHOST`, `PGPORT`, `PGUSER`, `PGDATABASE`, `PGSSLMODE`, `PGPASSWORD` |
| Desired schema input | stdin or `-f file` | `--file` (one file) |
| Config file | `--config` and `--config-inline` (YAML), repeatable | None found in `--help`; flags and environment |
| Schema scope | The whole database | One schema per run (`--schema`, default `public`) |
| Switches worth knowing | `--skip-view`, `--skip-extension`, `--skip-partition`, `--disable-ddl-transaction`, `--before-apply` | `--output-json`, `--output-sql`, `--output-human`, `--plan-*` set |
| Machine-readable output | Exit codes only | JSON plan |
| Secrets | Environment variable for the password | Environment variable or flag |

## Deployment

<a id="table-3"></a>
**Table 3 — Deployment**

| | sqldef | pgschema |
|---|---|---|
| Artefact | One static binary per engine: `psqldef` 27.6 MB, `mssqldef` 15.5 MB, `sqlite3def` 16.2 MB | One binary, 24.0 MB |
| Runtime dependencies | None (built with `CGO_ENABLED=0`) | None for a static build; planning uses an embedded Postgres unless `--plan-host` is set (how it is provided was not checked) |
| Where it runs from the wrapper | Child process through `ProcessToolRunner` on the host or in a container | Same |
| Build and pinning here | Go 1.26 or later needed; toolchain pinned in `Dockerfile.tools`, checksums in `SHA256SUMS` | Same image |
| CI fit | `--check` gives a drift gate (exit 2 when changes are pending) | `plan --output-json` as an artefact, `apply --plan` later |
| Footprint for a developer machine | Copy binaries to PATH | Copy one binary |

## Behaviour in the scenarios

Scores and the reasoning are in the [bake-off result](bake-off-result.md#scores). This table is the short form (P pass, G pass with a gap, F fail, — not run).

<a id="table-4"></a>
**Table 4 — Scenario results**

| Scenario | sqldef PostgreSQL | sqldef SQL Server | sqldef SQLite | pgschema PostgreSQL |
|---|---|---|---|---|
| S1 apply to empty | P | P | P | P |
| S2 second run changes nothing | P | F (small fixture) | P | P |
| S3 add column, index, view | P | P | P | P (noisy) |
| S4 rename a column | G (`@renamed` annotation works) | G (`@renamed` works) | G | F (data lost) |
| S5 drop | P (gated) | P (gated) | P (gated) | F (planned, no flag) |
| S6 narrow a type | F (no hazard note) | F (invalid T-SQL) | F | G (cast, no note) |
| S7 plan then apply | G (SQL text) | G (SQL text) | G | P (JSON plan file) |
| S8 script order | P | P | P | F |
| S9 function, view, trigger change | P | P | P | P |
| S10 extension | P | — | n/a | F |
| S11 drift | P | P | P (replayed as tests) | F |
| S12 usable from .NET | G (exit codes) | G (exit codes) | G | P (JSON) |

The eight SQLite scenarios are now replayed as `DevLocal` tests in the [spike](../../patterns-discovery/08-spikes/03-schema-bake-off.md) and agree with this table. The other columns are manual results; their harnesses are not built.

## Safety

<a id="table-5"></a>
**Table 5 — Safety defaults**

| | sqldef | pgschema |
|---|---|---|
| Destructive change without a flag | Skipped; needs `--enable-drop` | Planned; the interactive approval of `apply` is the only gate |
| Data-loss diagnostics | None | None found |
| Type narrowing or rewrite warning | None | None |
| Transactions | `psqldef` wraps the plan in BEGIN and COMMIT (seen in a dry run; other engines not checked); `--disable-ddl-transaction` opts out | One transaction (41 statements in S1) |
| Lock handling | Not exposed | Lock timeout in the plan |
| Drift detection | `--check`, exit 2 | Plan shows the difference |

For us the safety layer is the wrapper, not the tool: `ApplyOptions` refuses a drop unless allowed, and `PlanAnalyzer` reads the plan text for drops and narrowing. That is why a tool with a safe default (sqldef) needs the least wrapper code.

## Integration from .NET

- **sqldef:** `SqldefProvider` runs the binary through `IToolRunner`, reads `--check` exit codes (0 synced, 2 pending, 1 error) and treats the plan as text for `PlanAnalyzer`. No parsing library is needed, but the plan has no structure to rely on.
- **pgschema:** a JSON plan would let the wrapper inspect steps without parsing SQL, and a saved plan separates review from apply. It would need a provider of its own, PostgreSQL only.

## Pros and cons

<a id="table-6"></a>
**Table 6 — Pros and cons**

| Tool | Pros | Cons |
|---|---|---|
| sqldef | Safe by default (drops gated); one command shape for PostgreSQL, SQL Server and SQLite; static binaries with no dependencies; `--check` is a ready CI drift gate; objects (views, triggers, functions, extensions) handled on PostgreSQL; MIT | Plan is text only; no hazard report for type changes; renames need an annotation; a small SQL Server fixture re-planned primary keys on every run (unresolved); `ALTER COLUMN` with a default was invalid T-SQL |
| pgschema | JSON plan and a saved plan applied later; lock timeout; good fit for a reviewed PostgreSQL release; Apache 2.0 | PostgreSQL only; drops planned without a gate; script order must already be dependency order; extensions not planned; renames lose data; one schema per run |

## Licence and cost

<a id="table-7"></a>
**Table 7 — Licence and cost**

| | sqldef | pgschema |
|---|---|---|
| Licence | MIT (the parser directory has its own licence, not checked) | Apache 2.0 |
| REQ-008 (no paid feature for S1 to S9) | Met | Met |
| Maintenance score (10% of the plan) | Not assessed | Not assessed |
| Cost to own | Wrapper, annotations for renames, a T-SQL fix or workaround | A second wrapper for PostgreSQL only |

## Risks and unknowns

- **Maintenance is unscored.** Release cadence, issue response and the bus factor of each project were not assessed; sqldef and pgschema are single-organisation Go projects.
- **sqldef SQL Server** has one unresolved re-plan loop (small fixture) and one invalid `ALTER COLUMN`; DacFx stays the SQL Server baseline.
- **Only PostgreSQL and SQLite have a repeatable run;** SQL Server results are manual. The container stack for the missing columns now exists, the scenario harnesses do not.
- **Fixtures are small:** 20 plain tables plus one view; functions, triggers and extensions were exercised only on small cases.

## What would change the decision

2. A sqldef release that names type-change hazards, or fixes the SQL Server `ALTER COLUMN` and primary-key loop, would remove wrapper code.
3. A PostgreSQL-only deployment that wants reviewed, saved plans would justify a `PgSchemaProvider` beside `SqldefProvider` (the interface already allows it), with the drop gate in the wrapper.
4. A maintenance score under the plan's threshold for sqldef would move the decision toward build.
