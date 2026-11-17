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

## SQL-kind search provider

`TryAddPostgresSqlKindSearch(configuration)` registers `SqlKindSearchProvider` as an `ISearchProvider` (keyed `postgres-sql-kind`) over one query kind; it needs `TryAddPostgresMetadataQuery`. Settings under `OoBDev:Search:SqlKind`: `Kind` (the `[QueryKind]` name) and `IdFacet` (default `id`, the facet that becomes the item id).

- Free text becomes the kind's search words; filters map to facet operators (equals `eq`/`ne`, contains `like`/`nlike`, `gt`, `gte`, `lt`, `lte`); only the kind's filterable facets are supported, and a negated range, or two filters with the same operator on one facet, is refused with `NotSupportedException`.
- Hits are unranked (score 0), so hybrid fusion treats them by rank; the cursor is the engine's keyset cursor; like-item, near-text and near-vector anchors are refused.
- Tested with a fake `IQueryExecutor` (generated SQL and parameters); a run against the Docker PostgreSQL is part of the filter-parity Integration set that is not built yet.
