# Desired-State Database Tooling — Bake-off test plan

[← Testing strategy](testing-strategy.md) · [↑ Overview](README.md)

**Status:** Draft for owner review; a partial run is in [bake-off-result.md](bake-off-result.md) · **Date:** 2026-11-09

## Contents

- [Purpose](#purpose)
- [Candidates and engines](#candidates-and-engines)
- [Fixture schema](#fixture-schema)
- [Scenarios](#scenarios)
- [Scoring](#scoring)
- [Result record](#result-record)
- [Setup and safety](#setup-and-safety)

### List of Tables

1. [Table 5 — Candidates and engines](#table-5)
2. [Table 6 — Scenarios](#table-6)
3. [Table 7 — Scoring](#table-7)

## Purpose

Phase 0 of the [architecture](architecture.md#phases): decide per engine whether to wrap an existing diff tool or build one, covering REQ-001 to REQ-005 and REQ-008. The criteria come from the acceptance criteria of the [feature request](../../features/request/2026-10-28-desired-state-database-tooling.md). The earlier sqldef test plan in the notes repository was not available (owner, 2026-11-06, Q2), so this plan stands alone. Nothing runs until the owner has reviewed it.

## Candidates and engines

<a id="table-5"></a>**Table 5 — Candidates and engines**

| Candidate | Engines claimed | Notes to verify |
|-----------|-----------------|-----------------|
| sqldef | PostgreSQL, SQL Server, SQLite, MySQL | Licence, `--dry-run` plan output, `--enable-drop` gate, idempotent re-run |
| pgschema | PostgreSQL | Licence, plan and apply split, supported object types |
| Atlas | PostgreSQL, SQL Server, SQLite, others | Licence and which features need a paid tier |
| DacFx (reference) | SQL Server | Already used by `OoBDev.DacFx`; the baseline for SQL Server |

Oracle has no candidate; it stays a custom or hand-written decision (Q5). Versions of every tool are recorded at run time, never "latest".

## Fixture schema

One schema per engine written as plain `CREATE` scripts, taken from `OoBDev.Example.DB` for SQL Server and ported by hand for the others, with:

- tables with primary, foreign and unique keys and defaults;
- indexes (plain, unique, filtered or partial where the engine has them);
- views, one function and one trigger where the engine supports them;
- one extension or equivalent (for example `pgcrypto`) and one queue-like table, to show what the diff cannot model (layer 2);
- 20 tables or more, so ordering and dependency problems show.

## Scenarios

Each scenario runs against a throwaway container (SQLite in process) and is scored per tool and engine.

<a id="table-6"></a>**Table 6 — Scenarios**

| ID | Scenario | Requirement | Pass when |
|----|----------|-------------|-----------|
| S1 | Apply the scripts to an empty database | REQ-001 | The catalog matches the scripts (compare with a catalog dump) |
| S2 | Run the tool again without changes | REQ-004 | The plan is empty and nothing changes |
| S3 | Add a column, an index and a view | REQ-002 | The plan holds exactly those steps |
| S4 | Rename a column | REQ-002 | The plan is reviewable; record whether it is a rename or drop and add |
| S5 | Drop a column, drop a table, narrow a type | REQ-003 | Refused without an explicit flag, applied with it |
| S6 | Change a type that needs a table rewrite or lock | REQ-003 | The hazard is reported, not silent |
| S7 | Plan only, then apply | REQ-003 | The plan can be saved and reviewed before apply; apply uses it |
| S8 | A dependency cycle or out-of-order scripts | REQ-001 | Order is resolved or the failure message is clear |
| S9 | Triggers, functions and views change | REQ-002 | Changed objects are replaced; unchanged ones are not touched |
| S10 | Extension and queue objects | REQ-004 | Record what the tool ignores; ensure scripts run twice with no change |
| S11 | Hand-edited drift in the database | REQ-002 | The plan shows the drift, nothing is dropped without the flag |
| S12 | Run from a .NET test or process | REQ-006 | Exit codes and output are parseable (JSON, or a stable text form) |

## Scoring

<a id="table-7"></a>**Table 7 — Scoring**

| Criterion | Weight | Measure |
|-----------|--------|---------|
| Scenario pass rate | 40% | Scenarios passed of those that apply to the engine |
| Safety gates | 20% | S5, S6 and S7 all pass |
| Licence | Must | Free, MIT-compatible use, no paid feature needed for S1 to S9 (REQ-008); a fail here removes the tool |
| Output usable from .NET | 15% | S12; wrapping needs no scraping of free text |
| Object coverage | 15% | Views, functions, triggers and extensions handled or clearly reported |
| Maintenance | 10% | Last release date, open issues on the failing scenarios, release cadence |

Decision rule per engine: **buy** (wrap) when the licence passes, the safety gates pass and the score is 70% or more; **build** when the licence fails or the safety gates fail for every candidate; otherwise **buy with gaps**, listing what layer 2 or a custom step must cover. A tie is broken by the output criterion.

## Result record

The run writes `bake-off-result.md` in this folder: date, tool versions, container images, one row per tool and scenario (pass, fail or not applicable, with a note), the scores, the licence text and link per tool, and the decision per engine. REQ-005 and REQ-008 close when that file exists and is reviewed. Scripts and fixtures live under `src/Spikes` or a throwaway branch, as the [layering rule](../../../.claude/rules/layering-and-migration-plans.md) recommends for unclear shapes, and are deleted or promoted after the decision.

## Setup and safety

- Containers only (the existing `containers/testing` stack for SQL Server and Postgres, SQLite in process); no live or shared database, no real connection strings.
- Test credentials come from test parameters, never committed.
- Tool binaries are downloaded into a fresh directory per tool and never run from inside it; checksums and licence texts are recorded.
- The run is deterministic: the same scripts, the same order, a fresh database per scenario.
