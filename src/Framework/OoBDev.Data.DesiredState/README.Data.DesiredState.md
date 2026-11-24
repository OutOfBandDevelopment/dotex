# OoBDev.Data.DesiredState

Plans and applies the difference between a folder of `CREATE` scripts and a database. The scripts are the source of truth; a provider per engine computes what has to change and refuses unsafe steps without an explicit flag.

Design: [desired-state database tooling](../../../docs/design/DesiredStateDatabase/README.md). Decisions (owner, 2026-11-23): Q34 A, wrap sqldef and add the gates ourselves; Q35 A, Oracle through hand-written repeatable scripts.

## Providers

| Engine key | Provider | Tool |
|------------|----------|------|
| `postgres` | `SqldefProvider.ForPostgres` | `psqldef` (sqldef, MIT) |
| `sqlserver` | `SqldefProvider.ForSqlServer` | `mssqldef` (sqldef, MIT); `OoBDev.DacFx` stays the dacpac route |
| `sqlite` | `SqldefProvider.ForSqlite` | `sqlite3def` (sqldef, MIT) |
| `oracle` | `ScriptFolderProvider` | ordered repeatable scripts run through an `IScriptExecutor` you supply (the Oracle driver adapter) |

The binaries are not bundled: they must be on `PATH` (or pass the path to the `For*` method). `ProcessToolRunner` feeds the scripts on standard input and passes the password through the environment (`PGPASSWORD`, `MSSQL_PWD`), never on the command line.

## Safety gates

sqldef skips drops by itself but does not report type changes (bake-off S5, S6). `PlanAnalyzer` reads the plan and flags:

- `DataLoss`: `DROP TABLE|COLUMN|SCHEMA|DATABASE`, `TRUNCATE TABLE`;
- `Drop`: dropping an index, constraint, view, function, trigger and so on;
- `TypeChange`: a column type change (the old type is not in the plan, so any change is flagged as possibly narrowing).

`ApplyAsync` throws `UnsafePlanException` before running anything unless `ApplyOptions` permits each hazard (`AllowDataLoss` covers `DataLoss` and `Drop` and passes `--enable-drop`; `AllowTypeChange` covers `TypeChange`). The plan also lists the drops sqldef skipped (`-- Skipped:`).

## Use

```csharp
services.AddDesiredStateDatabase();          // sqldef providers, keyed by engine, and the factory
services.AddSingleton<IScriptExecutor, MyOracleExecutor>().AddOracleScripts();

var provider = factory.Create("postgres");
var desired = DesiredSchema.FromDirectory("db/schema");              // *.sql in file-name order
var plan = await provider.PlanAsync(desired, new DatabaseTarget("localhost", 5432, "app", "me", password));
await provider.ApplyAsync(desired, target, plan, new ApplyOptions());      // refuses drops and type changes
```

## Not built yet

The ensure-script runner with applied-hash records, the feature manifest compiler, the CLI verbs and an Oracle `IScriptExecutor` are in `BACKLOG.md`.

## Tests

Unit tests use a fake tool runner. The Integration test runs the real `sqlite3def` and ends Inconclusive when it is not on `PATH`.
