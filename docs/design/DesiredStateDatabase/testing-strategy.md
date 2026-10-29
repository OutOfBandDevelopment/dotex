# Desired-State Database Tooling — Testing strategy

[← API design](api-design.md) · [↑ Overview](README.md)

**Status:** Draft · **Date:** 2026-10-29

## Approach

Plan generation and hazard detection are Unit tests on fixture schemas (80% coverage of Framework code). Apply and ensure-script tests are Integration against the stack containers (SQL Server, Postgres where present, SQLite in process); Oracle needs a container whose availability is unknown and stays `DevLocal` until then.

### List of Tables

1. [Table 4 — Requirement coverage](#table-4)

<a id="table-4"></a>**Table 4 — Requirement coverage**

| Requirement | Category | Test idea |
|-------------|----------|-----------|
| REQ-001, REQ-002 | Integration | apply scripts to an empty database, change one, plan contains one step |
| REQ-003 | Unit | dropping a column yields a hazard and `ApplyAsync` throws without the flag |
| REQ-004 | Integration | run ensure scripts twice, second run reports no change |
| REQ-005, REQ-008 | Manual record | bake-off result document under `docs/design/DesiredStateDatabase/` |
| REQ-006 | Unit | providers resolve by engine key |
| REQ-007 | Unit | manifest compiles to expected script text |
