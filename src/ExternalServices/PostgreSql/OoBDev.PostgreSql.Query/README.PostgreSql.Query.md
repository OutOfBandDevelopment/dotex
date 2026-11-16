# OoBDev.PostgreSql.Query

The PostgreSQL adapter of [OoBDev.Query.Metadata](../../../Framework/OoBDev.Query.Metadata/README.Query.Metadata.md). Design: [docs/design/MetadataQuery](../../../../docs/design/MetadataQuery/README.md).

## Use

```csharp
services.TryAddPostgresMetadataQuery(configuration, typeof(PhotoRow).Assembly);   // Query:Metadata:* and Query:Postgres:*
```

Registers `PostgresQueryDialect` and `NpgsqlQueryExecutor` (a lazily created `NpgsqlDataSource`; values are always sent as parameters) beside the engine.

## Dialect

`ILIKE ... ESCAPE '\'` with `%`, `_` and `\` escaped in the pattern, `= ANY(array)` for `in` and array facets, `NULLS LAST` ordering, `LIMIT n`, and `to_char` date parts (`year`, `month`, `day`).

## Options (`Query:Postgres`)

| Key | Default | Meaning |
|-----|---------|---------|
| `ConnectionString` | required | Database holding the kinds' tables (put `Search Path=` in it to select a schema) |
| `CommandTimeoutSeconds` | 30 (1 to 600) | Command timeout |

## Tests

Unit tests cover the dialect and registration. Integration tests run against the `pgvector` service of the test stack (`PGVECTOR_CONNECTION_STRING`) in a throwaway schema per test: filters and count, wildcard text taken literally, injection text, keyset paging with a row inserted between pages, and facet counts.
