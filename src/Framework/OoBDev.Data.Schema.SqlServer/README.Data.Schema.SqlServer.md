# OoBDev.Data.Schema.SqlServer

SQL project (`Microsoft.Build.Sql`) that builds `OoBDev.Data.Schema.SqlServer.dacpac`: the shipped SQL Server schema for the EF stores, for databases deployed with `sqlpackage` (desired state) instead of EF migrations. Postgres and Sqlite use the EF migrations projects ([migrations plan](../../../docs/design/CqrsEventSourcing/migrations-plan.md)).

| Table script | EF context |
| --- | --- |
| `Tables/RightAudit.sql` | `RightAuditDbContext` (`OoBDev.AspNetCore.EntityFrameworkCore`) |
| `Tables/Suppressions.sql` | `SuppressionsDbContext` (`OoBDev.Communications.EntityFrameworkCore`) |
| `Tables/Schedule.sql` | `ScheduleDbContext` (`OoBDev.ComplexEvents.EntityFrameworkCore`) |
| `Tables/ComplexEventLastRun.sql` | `LastRunRow` in `ScheduleDbContext` (last-run times of recurring schedules; created on the SQL Server test container 2026-11-23) |

The scripts are generated from the models (`Database.GenerateCreateScript()` with the SqlServer provider). `OoBDev.Data.Migrations.Tests` (`SqlServerSchemaTests`, Unit) fails when a script no longer matches its model; regenerate with `REGENERATE_SQL_SCHEMA=1 dotnet test src/Framework/OoBDev.Data.Migrations.Tests --filter "FullyQualifiedName~SqlServerSchema"`.

Build: `dotnet build src/Framework/OoBDev.Data.Schema.SqlServer` (the dacpac lands in `bin/Debug`). Not a NuGet package.

Deploy check (2026-11-20): `sqlpackage /Action:Publish` of the built dacpac into a new database on the `oobd-test-sqlserver` container (run from the `dotnet/sdk:10.0` image on `oobd-integration-test-net`, because the Windows `sqlpackage` 170 tool failed to start on this host) created `RightAudit`, `Suppressions` and `ComplexEventSchedule`; the throwaway database was dropped afterwards.
