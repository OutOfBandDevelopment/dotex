# OoBDev.EventSourcing.Postgres

PostgreSQL database package for the Entity Framework Core event and entity stores (`OoBDev.EventSourcing.EntityFrameworkCore`; design in `docs/design/CqrsEventSourcing/`).

- `services.TryAddPostgresEventSourcing(connectionString)` registers the stores under the key `postgres` (`PostgresGlobals.ProviderKey`); select them with `OoBDev:EventSourcing:EventStore:Type` and `OoBDev:EventSourcing:EntityStore:Type`.
- The schema comes from the EF migration `InitialCreate` in this project (`context.Database.MigrateAsync()`); JSON columns are `jsonb` and the position is an identity column.
- Add a migration after a model change: `dotnet ef migrations add <Name> --context EventSourcingDbContext --output-dir Migrations` from this folder (a test fails while the model and the migrations differ).
- Tests against a real server need the PostgreSQL service in the integration stack (backlog).
