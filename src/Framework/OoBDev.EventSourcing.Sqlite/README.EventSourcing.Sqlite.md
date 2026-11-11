# OoBDev.EventSourcing.Sqlite

SQLite database package for the Entity Framework Core event and entity stores (`OoBDev.EventSourcing.EntityFrameworkCore`; design in `docs/design/CqrsEventSourcing/`).

- `services.TryAddSqliteEventSourcing(connectionString)` registers the stores under the key `sqlite` (`SqliteGlobals.ProviderKey`); select them with `OoBDev:EventSourcing:EventStore:Type` and `OoBDev:EventSourcing:EntityStore:Type`.
- The schema comes from the EF migration `InitialCreate` in this project (`context.Database.MigrateAsync()`); JSON columns are `TEXT` with a `json_valid` check.
- Add a migration after a model change: `dotnet ef migrations add <Name> --context EventSourcingDbContext --output-dir Migrations` from this folder (a test fails while the model and the migrations differ).
