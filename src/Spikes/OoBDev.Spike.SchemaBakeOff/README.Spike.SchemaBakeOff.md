# OoBDev.Spike.SchemaBakeOff

## Summary

Spike: replay the desired-state database bake-off ([test plan](../../../docs/design/DesiredStateDatabase/bake-off-test-plan.md)) as repeatable tests instead of a one-off scratch run. The first column is sqldef on SQLite, which needs no container. Findings are in [the spike report](../../../docs/patterns-discovery/08-spikes/03-schema-bake-off.md).

This is throwaway evidence, not a product: it is not packaged and not part of `OoBDev.slnx`.

## Contents

| File | Purpose |
|------|---------|
| `SqliteScenarioHarness.cs` | A throwaway SQLite file database and the `sqlite3def` runs (`--dry-run`, `--check`, apply with or without `--enable-drop`), plus catalog reads |
| `SqldefSqliteScenarioTests.cs` | Eight `DevLocal` tests for S1 to S3, S5, S6, S8, S9, S11 and S12; Inconclusive when `sqlite3def` is not on PATH |

## Running

```bash
go install github.com/sqldef/sqldef/v3/cmd/sqlite3def@latest
dotnet test src/Spikes/OoBDev.Spike.SchemaBakeOff
```

## Not covered yet

pgschema, Atlas and the PostgreSQL and SQL Server columns still need their binaries and containers (see the report).
