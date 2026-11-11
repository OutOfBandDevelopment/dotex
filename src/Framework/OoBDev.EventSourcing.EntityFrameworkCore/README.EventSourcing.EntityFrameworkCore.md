# OoBDev.EventSourcing.EntityFrameworkCore

Entity Framework Core implementation of `IEventStore` and `IEntityStore` (design in `docs/design/CqrsEventSourcing/`). One model serves PostgreSQL, SQL Server and SQLite; the provider packages (`OoBDev.EventSourcing.Sqlite`, `.Postgres`, `.SqlServer`) choose the database.

- `EventSourcingDbContext`: tables `Events` (identity `Position`, unique `(StreamId, Sequence)`) and `Entities` (key `(Type, Key)`, `Version`). JSON columns are `jsonb` on PostgreSQL, `nvarchar(max)` with an `ISJSON` check on SQL Server and `TEXT` with a `json_valid` check on SQLite.
- `EfEventStore`: append checks the expected sequence and relies on the unique index for concurrent writers; a lost race returns a conflict result.
- `EfEntityStore`: a new entity is an insert (a duplicate key is a conflict); an update or delete is one statement conditional on the version.
- `TryAddEntityFrameworkEventSourcing(providerKey, configure)`: registers the context factory and both stores under the key the database package passes (`sqlite`, `postgres` or `sql-server`). Select them with `OoBDev:EventSourcing:EventStore:Type` and `OoBDev:EventSourcing:EntityStore:Type`.
