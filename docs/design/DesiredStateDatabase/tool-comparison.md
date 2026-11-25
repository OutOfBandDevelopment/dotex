# Desired-State Database Tooling — Tool comparison

[← Bake-off result](bake-off-result.md) · [↑ Overview](README.md)

**Status:** current as of 2026-11-24. Sources: the [bake-off result](bake-off-result.md) (run 2026-11-20 to 2026-11-23), the `--help` output of the tools built into [`containers/bakeoff`](../../../containers/bakeoff/README.md) on 2026-11-24, and the wrapper code in `OoBDev.Data.DesiredState`. Where a statement was not checked against the tool, it says so.

This report compares the three candidates (sqldef, pgschema, Atlas) on the points that decide adoption: what each one does, how it is configured, how it is deployed, how it behaves in the scenarios, and what it costs to own. The decision is already made (Q34 A: wrap sqldef, with a drop gate and a type-change check added by the wrapper); this report records why and what would change it.

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
- **Atlas** is the most capable product, but the free binary tested in the bake-off refuses views and extensions without a vendor login and gates its data-loss diagnostics behind it. That breaks REQ-008, so it was removed. The community image now in the bake-off stack has not been re-run.
- **No tool does Oracle** (Q35: hand-written repeatable scripts), and none reports the hazard of narrowing a type (S6), so the wrapper's `PlanAnalyzer` is needed whichever tool is wrapped.

## Identity and scope

<a id="table-1"></a>
**Table 1 — Identity and scope**

| | sqldef | pgschema | Atlas |
|---|---|---|---|
| Version tested | 3.11.30 | 1.14.1 | v1.4.2 (vendor binary, 2026-11-23); v1.4.1 canary (community image, 2026-11-24, not yet scored) |
| Language and build | Go; built here from the pinned tag | Go; built here from the pinned tag | Go; vendor binary and `arigaio/atlas:latest-community` image |
| Engines | PostgreSQL (`psqldef`), SQL Server (`mssqldef`), SQLite (`sqlite3def`), MySQL (`mysqldef`, not tested) | PostgreSQL only | Many (PostgreSQL tested; SQLite and SQL Server not run) |
| Desired state is | Plain `CREATE` statements in SQL | Plain `CREATE` statements in SQL | HCL, SQL files or another database (`--to`) |
| Needs a second database to plan | No | Optional: an embedded Postgres by default, or `--plan-host` | Yes: a dev database (`--dev-url`) |
| Fits our model (plain `CREATE` scripts) | Yes | Yes | Only with SQL input; its own model is HCL |

## Configuration

<a id="table-2"></a>
**Table 2 — Configuration**

| | sqldef | pgschema | Atlas |
|---|---|---|---|
| Connection | Flags `-h -p -U` plus the database name; `PGPASSWORD` honoured by `psqldef` | Flags `--host --port --user --db`, or `PGHOST`, `PGPORT`, `PGUSER`, `PGDATABASE`, `PGSSLMODE`, `PGPASSWORD` | A URL per resource (`-u`, `--to`, `--dev-url`) |
| Desired schema input | stdin or `-f file` | `--file` (one file) | `--to file://...` (several allowed) |
| Config file | `--config` and `--config-inline` (YAML), repeatable | None found in `--help`; flags and environment | Project file `atlas.hcl` with environments (`--env`) |
| Schema scope | The whole database | One schema per run (`--schema`, default `public`) | `--schema`, `--include`, `--exclude` globs |
| Switches worth knowing | `--skip-view`, `--skip-extension`, `--skip-partition`, `--disable-ddl-transaction`, `--before-apply` | `--output-json`, `--output-sql`, `--output-human`, `--plan-*` set | `--tx-mode`, `--lock-timeout`, `--plan`, `--edit`, `--format` template |
| Machine-readable output | Exit codes only | JSON plan | Go-template `--format` (JSON possible) |
| Secrets | Environment variable for the password | Environment variable or flag | URL (credentials in the URL) or the project file |

## Deployment

<a id="table-3"></a>
**Table 3 — Deployment**

| | sqldef | pgschema | Atlas |
|---|---|---|---|
| Artefact | One static binary per engine: `psqldef` 27.6 MB, `mssqldef` 15.5 MB, `sqlite3def` 16.2 MB | One binary, 24.0 MB | Vendor binary, or a 63.6 MB container image (community) |
| Runtime dependencies | None (built with `CGO_ENABLED=0`) | None for a static build; planning uses an embedded Postgres unless `--plan-host` is set (how it is provided was not checked) | A dev database, normally a Docker container (`docker://postgres/15/dev`) or a second server |
| Where it runs from the wrapper | Child process through `ProcessToolRunner` on the host or in a container | Same | Same, plus the dev database to provision |
| Build and pinning here | Go 1.26 or later needed; toolchain pinned in `Dockerfile.tools`, checksums in `SHA256SUMS` | Same image | Image tag; the vendor binary is a download without a recorded source build |
| CI fit | `--check` gives a drift gate (exit 2 when changes are pending) | `plan --output-json` as an artefact, `apply --plan` later | `schema diff` and `--dry-run`; apply re-plans in the edition tested |
| Footprint for a developer machine | Copy binaries to PATH | Copy one binary | Install the CLI and Docker for the dev database |

## Behaviour in the scenarios

Scores and the reasoning are in the [bake-off result](bake-off-result.md#scores). This table is the short form (P pass, G pass with a gap, F fail, — not run).

<a id="table-4"></a>
**Table 4 — Scenario results**

| Scenario | sqldef PostgreSQL | sqldef SQL Server | sqldef SQLite | pgschema PostgreSQL | Atlas PostgreSQL (vendor binary) |
|---|---|---|---|---|---|
| S1 apply to empty | P | P | P | P | P |
| S2 second run changes nothing | P | F (small fixture) | P | P | P |
| S3 add column, index, view | P | P | P | P (noisy) | P without views |
| S4 rename a column | G (`@renamed` annotation works) | G (`@renamed` works) | G | F (data lost) | F (data lost) |
| S5 drop | P (gated) | P (gated) | P (gated) | F (planned, no flag) | F (planned, no flag) |
| S6 narrow a type | F (no hazard note) | F (invalid T-SQL) | F | G (cast, no note) | — |
| S7 plan then apply | G (SQL text) | G (SQL text) | G | P (JSON plan file) | G (re-plans) |
| S8 script order | P | P | P | F | — |
| S9 function, view, trigger change | P | P | P | P | F (views need login) |
| S10 extension | P | — | n/a | F | F (login) |
| S11 drift | P | P | P (replayed as tests) | F | Detected, ungated |
| S12 usable from .NET | G (exit codes) | G (exit codes) | G | P (JSON) | P (JSON) |

The eight SQLite scenarios are now replayed as `DevLocal` tests in the [spike](../../patterns-discovery/08-spikes/03-schema-bake-off.md) and agree with this table. The other columns are manual results; their harnesses are not built.

## Safety

<a id="table-5"></a>
**Table 5 — Safety defaults**

| | sqldef | pgschema | Atlas |
|---|---|---|---|
| Destructive change without a flag | Skipped; needs `--enable-drop` | Planned; the interactive approval of `apply` is the only gate | Planned; `--auto-approve` would run it |
| Data-loss diagnostics | None | None found | Present but login-gated in the vendor binary |
| Type narrowing or rewrite warning | None | None | Login-gated |
| Transactions | `psqldef` wraps the plan in BEGIN and COMMIT (seen in a dry run; other engines not checked); `--disable-ddl-transaction` opts out | One transaction (41 statements in S1) | `--tx-mode file` by default |
| Lock handling | Not exposed | Lock timeout in the plan | `--lock-timeout`, default 10 s |
| Drift detection | `--check`, exit 2 | Plan shows the difference | `schema diff` |

For us the safety layer is the wrapper, not the tool: `ApplyOptions` refuses a drop unless allowed, and `PlanAnalyzer` reads the plan text for drops and narrowing. That is why a tool with a safe default (sqldef) needs the least wrapper code.

## Integration from .NET

- **sqldef:** `SqldefProvider` runs the binary through `IToolRunner`, reads `--check` exit codes (0 synced, 2 pending, 1 error) and treats the plan as text for `PlanAnalyzer`. No parsing library is needed, but the plan has no structure to rely on.
- **pgschema:** a JSON plan would let the wrapper inspect steps without parsing SQL, and a saved plan separates review from apply. It would need a provider of its own, PostgreSQL only.
- **Atlas:** JSON output and a project file, but a dev database and licence handling to carry. No provider was written.

## Pros and cons

<a id="table-6"></a>
**Table 6 — Pros and cons**

| Tool | Pros | Cons |
|---|---|---|
| sqldef | Safe by default (drops gated); one command shape for PostgreSQL, SQL Server and SQLite; static binaries with no dependencies; `--check` is a ready CI drift gate; objects (views, triggers, functions, extensions) handled on PostgreSQL; MIT | Plan is text only; no hazard report for type changes; renames need an annotation; a small SQL Server fixture re-planned primary keys on every run (unresolved); `ALTER COLUMN` with a default was invalid T-SQL |
| pgschema | JSON plan and a saved plan applied later; lock timeout; good fit for a reviewed PostgreSQL release; Apache 2.0 | PostgreSQL only; drops planned without a gate; script order must already be dependency order; extensions not planned; renames lose data; one schema per run |
| Atlas | Many engines; mature project model (environments, `--include`, `--exclude`); JSON output; lock timeout and transaction mode | Free binary gated views, extensions and diagnostics behind a login; needs a dev database; apply re-plans; drops ungated; HCL model is a second schema language unless SQL input is used |

## Licence and cost

<a id="table-7"></a>
**Table 7 — Licence and cost**

| | sqldef | pgschema | Atlas |
|---|---|---|---|
| Licence | MIT (the parser directory has its own licence, not checked) | Apache 2.0 | Community edition open source; vendor binary under the vendor's terms (text not assessed beyond the errors seen) |
| REQ-008 (no paid feature for S1 to S9) | Met | Met | Not met with the vendor binary; unknown for the community image |
| Maintenance score (10% of the plan) | Not assessed | Not assessed | Not assessed |
| Cost to own | Wrapper, annotations for renames, a T-SQL fix or workaround | A second wrapper for PostgreSQL only | Licence management, dev database in CI |

## Risks and unknowns

- **Maintenance is unscored.** Release cadence, issue response and the bus factor of each project were not assessed; sqldef and pgschema are single-organisation Go projects.
- **sqldef SQL Server** has one unresolved re-plan loop (small fixture) and one invalid `ALTER COLUMN`; DacFx stays the SQL Server baseline.
- **Atlas community image** may behave differently from the vendor binary; its S3, S9 and S10 need a fresh run before the licence finding is revisited.
- **Only PostgreSQL and SQLite have a repeatable run;** SQL Server results are manual. The container stack for the missing columns now exists, the scenario harnesses do not.
- **Fixtures are small:** 20 plain tables plus one view; functions, triggers and extensions were exercised only on small cases.

## What would change the decision

1. A fresh Atlas community run passing S3 (with a view), S9 and S10 without a login would reopen it, mainly for its machine-readable output.
2. A sqldef release that names type-change hazards, or fixes the SQL Server `ALTER COLUMN` and primary-key loop, would remove wrapper code.
3. A PostgreSQL-only deployment that wants reviewed, saved plans would justify a `PgSchemaProvider` beside `SqldefProvider` (the interface already allows it), with the drop gate in the wrapper.
4. A maintenance score under the plan's threshold for sqldef would move the decision toward build.
