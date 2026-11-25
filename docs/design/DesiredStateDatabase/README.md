# Desired-State Database Tooling — Overview

**Status:** Draft · **Date:** 2026-10-29 · **Request:** [Desired-state database tooling](../../features/request/2026-10-28-desired-state-database-tooling.md) · **Related:** `OoBDev.DacFx`, `OoBDev.DacPacCompiler.Cli`, `OoBDev.ComplexEvents.DatabaseExtensions`, [design standard](../../patterns-discovery/06-design-document-standard.md)

Derive migrations from plain `CREATE` scripts for SQL Server, Postgres, SQLite and Oracle, and manage the objects a schema diff cannot model (extensions, queues, event streams) with idempotent ensure scripts.

## Contents

1. [Requirements](requirements.md)
2. [Architecture](architecture.md)
3. [API design](api-design.md)
4. [Testing strategy](testing-strategy.md)
5. [Tool comparison](tool-comparison.md) (sqldef, pgschema, Atlas: configuration, deployment, safety, pros and cons)

### List of Tables

1. [Table 1 — Requirements](requirements.md#table-1)
2. [Table 2 — Phases](architecture.md#table-2)
3. [Table 3 — Alternatives](architecture.md#table-3)
4. [Table 4 — Requirement coverage](testing-strategy.md#table-4)

### List of Figures

1. [Figure 1 — Layers](architecture.md#figure-1)

## Decisions and status (2026-11-23)

Owner answers: **Q34 A** buy with gaps (wrap sqldef for PostgreSQL, SQL Server and SQLite; the wrapper adds the drop gate and the type-change check), **Q35 A** Oracle through hand-written repeatable scripts run by the same apply step with the same gate. Built in `OoBDev.Data.DesiredState` ([readme](../../../src/Framework/OoBDev.Data.DesiredState/README.Data.DesiredState.md)): `IDesiredStateProvider`, `SqldefProvider`, `ScriptFolderProvider`, `PlanAnalyzer`, `ApplyOptions`. Not built: the ensure-script runner, the manifest compiler, the CLI.

## Summary

The work starts with a recorded buy-or-build bake-off (plan: [bake-off-test-plan.md](bake-off-test-plan.md)) of sqldef, pgschema and Atlas on a real schema (phase 0). Nothing else is built until it has a result per engine. Layers 1 to 3 (diff provider, ensure scripts, feature manifest) follow behind a provider interface. The open choices are in `OPEN-QUESTIONS.md` (Q5): schema scope, wrap versus write, and the stack.
