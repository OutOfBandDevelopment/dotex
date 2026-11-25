# OoBDev.EventSourcing.Sqlite

SQLite database package for the Entity Framework Core event and entity stores (`OoBDev.EventSourcing.EntityFrameworkCore`; design in `docs/design/CqrsEventSourcing/`).

- `services.TryAddSqliteEventSourcing(connectionString)` registers the stores under the key `sqlite` (`SqliteGlobals.ProviderKey`); select them with `OoBDev:EventSourcing:EventStore:Type` and `OoBDev:EventSourcing:EntityStore:Type`.
- The schema is a final state, not a migration chain: the script is generated from the model and committed under `Schema/sqlite` of `OoBDev.EventSourcing.EntityFrameworkCore.Tests` (a test fails while it differs), and `OoBDev.Data.DesiredState` brings a database file to it (`EfDesiredStateDeployer.EnsureAsync(context)`; needs `sqlite3def` on PATH, and the database must be a file). JSON columns are `TEXT` with a `json_valid` check.
- After a model change, regenerate the script with `REGENERATE_SQL_SCHEMA=1`; there is no `dotnet ef migrations add` (owner, 2026-11-24).
