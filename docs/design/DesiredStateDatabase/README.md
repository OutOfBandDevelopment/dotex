# Desired-State Database Tooling — Overview

**Status:** Draft · **Date:** 2026-10-29 · **Request:** [Desired-state database tooling](../../features/request/2026-10-28-desired-state-database-tooling.md) · **Related:** `OoBDev.DacFx`, `OoBDev.DacPacCompiler.Cli`, `OoBDev.ComplexEvents.DatabaseExtensions`, [design standard](../../patterns-discovery/06-design-document-standard.md)

Derive migrations from plain `CREATE` scripts for SQL Server, Postgres and SQLite, and manage the objects a schema diff cannot model (extensions, queues, event streams) with idempotent ensure scripts.

## Contents

1. [Requirements](requirements.md)
2. [Architecture](architecture.md)
3. [API design](api-design.md)
4. [Testing strategy](testing-strategy.md)
5. [Final-state plan](final-state-plan.md) (replace EF migrations with final-state scripts; confirmed 2026-11-25)
6. [Tool comparison](tool-comparison.md) (sqldef and pgschema: configuration, deployment, safety, pros and cons)
7. [Build plan for PostgreSQL and SQLite](plan.md) (EF bridge, host hook, package and refactor log, canonical model; proposed)

### List of Tables

1. [Table 1 — Requirements](requirements.md#table-1)
2. [Table 2 — Phases](architecture.md#table-2)
3. [Table 3 — Alternatives](architecture.md#table-3)
4. [Table 4 — Requirement coverage](testing-strategy.md#table-4)

### List of Figures

1. [Figure 1 — Layers](architecture.md#figure-1)

## Decisions and status (2026-11-23)

Owner answers: **Q34 A** buy with gaps (wrap sqldef for PostgreSQL, SQL Server and SQLite; the wrapper adds the drop gate and the type-change check), **Oracle rejected** (owner, 2026-11-24: no Oracle provider, executor, driver or container; this replaces Q35 A and withdraws Q36). Built in `OoBDev.Data.DesiredState` ([readme](../../../src/Framework/OoBDev.Data.DesiredState/README.Data.DesiredState.md)): `IDesiredStateProvider`, `SqldefProvider`, `PlanAnalyzer`, `ApplyOptions`. Also built: the ensure-script runner, the manifest compiler and the CLI.

## Summary

The work starts with a recorded buy-or-build bake-off (plan: [bake-off-test-plan.md](bake-off-test-plan.md)) of sqldef, pgschema and Atlas (Atlas dropped by the owner, 2026-11-24) on a real schema (phase 0). Nothing else is built until it has a result per engine. Layers 1 to 3 (diff provider, ensure scripts, feature manifest) follow behind a provider interface. The open choices are in `OPEN-QUESTIONS.md` (Q5): schema scope, wrap versus write, and the stack.
