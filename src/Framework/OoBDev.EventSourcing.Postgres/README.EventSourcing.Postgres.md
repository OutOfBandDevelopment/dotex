# OoBDev.EventSourcing.Postgres

PostgreSQL database package for the Entity Framework Core event and entity stores (`OoBDev.EventSourcing.EntityFrameworkCore`; design in `docs/design/CqrsEventSourcing/`).

- `services.TryAddPostgresEventSourcing(connectionString)` registers the stores under the key `postgres` (`PostgresGlobals.ProviderKey`); select them with `OoBDev:EventSourcing:EventStore:Type` and `OoBDev:EventSourcing:EntityStore:Type`.
- The schema is a final state, not a migration chain: the script is generated from the model and committed under `Schema/postgres` of `OoBDev.EventSourcing.EntityFrameworkCore.Tests` (a test fails while it differs), and `OoBDev.Data.DesiredState` brings a database to it (`EfDesiredStateDeployer.EnsureAsync(context)`; needs `psqldef` on PATH). JSON columns are `jsonb` and the position is an identity column.
- After a model change, regenerate the script with `REGENERATE_SQL_SCHEMA=1`; there is no `dotnet ef migrations add` (owner, 2026-11-24).
- Tests against a real server need the PostgreSQL service in the integration stack (backlog).
