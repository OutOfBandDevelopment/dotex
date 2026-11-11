# OoBDev.EventSourcing.SqlServer

SQL Server database package for the Entity Framework Core event and entity stores (`OoBDev.EventSourcing.EntityFrameworkCore`; design in `docs/design/CqrsEventSourcing/`).

- `services.TryAddSqlServerEventSourcing(connectionString)` registers the stores under the key `sql-server` (`SqlServerGlobals.ProviderKey`); select them with `OoBDev:EventSourcing:EventStore:Type` and `OoBDev:EventSourcing:EntityStore:Type`.
- The schema is deployed from the dacpac of `OoBDev.EventSourcing.SqlServer.Database` (no EF migrations); a test compares that dacpac with the EF model.
- JSON columns are `nvarchar(max)` with an `ISJSON` check.
