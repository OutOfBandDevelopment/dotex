# OoBDev.Data.DesiredState

Plans and applies the difference between a folder of `CREATE` scripts and a database. The scripts are the source of truth; a provider per engine computes what has to change and refuses unsafe steps without an explicit flag.

Design: [desired-state database tooling](../../../docs/design/DesiredStateDatabase/README.md). Decisions (owner, 2026-11-23): Q34 A, wrap sqldef and add the gates ourselves; Oracle support rejected (owner, 2026-11-24: the engines are PostgreSQL, SQL Server and SQLite).

## Providers

| Engine key | Provider | Tool |
|------------|----------|------|
| `postgres` | `SqldefProvider.ForPostgres` | `psqldef` (sqldef, MIT) |
| `sqlserver` | `SqldefProvider.ForSqlServer` | `mssqldef` (sqldef, MIT); `OoBDev.DacFx` stays the dacpac route |
| `sqlite` | `SqldefProvider.ForSqlite` | `sqlite3def` (sqldef, MIT) |

The binaries are not in the package: they are found on `PATH` (or pass the path to the `For*` method), or `services.AddSqldefDownload()` fetches a missing tool on first use: `SqldefInstaller` downloads the pinned sqldef release (3.11.30) from GitHub, checks its SHA-256 against a pinned table and extracts it once into `%LOCALAPPDATA%/OoBDev/sqldef/<version>` (a mismatch extracts nothing); a tool already on `PATH` wins. `ProcessToolRunner` feeds the scripts on standard input and passes the password through the environment (`PGPASSWORD`, `MSSQL_PWD`), never on the command line.

## Safety gates

sqldef skips drops by itself but does not report type changes (bake-off S5, S6). `PlanAnalyzer` reads the plan and flags:

- `DataLoss`: `DROP TABLE|COLUMN|SCHEMA|DATABASE`, `TRUNCATE TABLE`;
- `Drop`: dropping an index, constraint, view, function, trigger and so on;
- `TypeChange`: a column type change (the old type is not in the plan, so any change is flagged as possibly narrowing).

`ApplyAsync` throws `UnsafePlanException` before running anything unless `ApplyOptions` permits each hazard (`AllowDataLoss` covers `DataLoss` and `Drop` and passes `--enable-drop`; `AllowTypeChange` covers `TypeChange`). The plan also lists the drops sqldef skipped (`-- Skipped:`).

## Use

```csharp
services.AddDesiredStateDatabase();          // sqldef providers, keyed by engine, and the factory

var provider = factory.Create("postgres");
var desired = DesiredSchema.FromDirectory("db/schema");              // *.sql in file-name order
var plan = await provider.PlanAsync(desired, new DatabaseTarget("localhost", 5432, "app", "me", password));
await provider.ApplyAsync(desired, target, plan, new ApplyOptions());      // refuses drops and type changes
```

The command line is [`desired-state`](../../Tools/OoBDev.DesiredState.Cli/README.DesiredState.Cli.md).

## EF Core contexts

`EfDesiredStateDeployer.EnsureAsync(context, options, ensureScripts)` replaces `Database.MigrateAsync()` for PostgreSQL (Npgsql) and SQLite contexts: the create script of the model (`GenerateCreateScript()`) is the desired state, the provider plans the difference, the gates apply, then the ensure scripts run. A model with `HasData` seed rows is refused (move the rows to a `*.once.sql` script); an in-memory SQLite database is refused (the tool is another process); SQL Server keeps its dacpac route. `AllowDataLoss` applies drops that sqldef skipped in its plan.

## Ensure scripts

`EnsureScriptRunner` runs ensure scripts (extensions, queues, event streams) in file-name order through an `IScriptExecutor`. Scripts must be idempotent and run every time; a script named `*.once.sql` runs only until its name and SHA-512 content hash are recorded in the `IAppliedScriptStore` (in-memory by default; a database-table store is a consumer's adapter). The result is an `EnsureReport` of ran and skipped scripts.

## Schema packages (build plan phase D)

`SchemaPackage` is the dacpac equivalent: schema scripts, pre- and post-deploy scripts and a refactor log (`RefactorEntry`: id, table, SQL). `SchemaPackageFormat` reads and writes the zip (`manifest.json`, `schema/`, `predeploy/`, `postdeploy/`, `refactorlog.json`). `SchemaPackageDeployer.DeployAsync(package, engine, target, options)` runs the pre-deploy scripts, then each unapplied refactor entry (skipped and only recorded when the plan creates its table, so a fresh database has nothing to rename), re-plans, applies with the gates, runs the post-deploy scripts and records the package hash. Records are kept by the `IAppliedScriptStore`; `AdoAppliedScriptStore` keeps them in `__desired_state_history` (add `AdoAppliedScriptStore.SchemaScript` to the package schema when drops are permitted).

## Not built yet

Publish profile files. Oracle is rejected (owner, 2026-11-24), so there is no Oracle provider, executor or driver.

## Tests

Unit tests use a fake tool runner. The Integration test runs the real `sqlite3def` and ends Inconclusive when it is not on `PATH`.

## Feature manifest

`FeatureManifest.Parse("""{ "extensions": ["pgcrypto"], "queues": ["orders"], "streams": ["order_events"] }""")` and `ManifestCompiler.Compile(manifest, "postgres")` give idempotent ensure scripts (`001_extensions.sql`, `020_queues.sql`, `030_streams.sql`) for `EnsureScriptRunner`. Queues and streams are portable tables (`queue_{name}`, `stream_{name}`); extensions exist for Postgres only; names are validated identifiers.

## Running ensure scripts

`AdoScriptExecutor` is the `IScriptExecutor` for any ADO.NET provider: pass a factory that opens the connection for a `DatabaseTarget` (Npgsql, SqlClient, Microsoft.Data.Sqlite). It needs no driver package here.
