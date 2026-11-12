# Metadata-driven query engine

Status: **design written 2026-11-12; placement confirmed by the owner 2026-11-12 (option A: database-free Framework project plus a PostgreSQL adapter), nothing built.** Request: [metadata-driven query engine](../../features/request/2026-11-11-metadata-driven-query-engine.md). Transport options: [OData over QUERY](../ODataQuery/README.md) and [HTTP QUERY verb](../HttpQueryVerb/README.md).

## Contents

1. [Need](#need)
2. [Superset comparison](#superset-comparison)
3. [Design](#design)
4. [Migration plan](#migration-plan)
5. [Testing strategy](#testing-strategy)
6. [Not in the first slice](#not-in-the-first-slice)

## Need

One request shape answers every read screen. A *kind* (a row class) declares by attributes what may be asked of it (filters, sort, paging, similarity, grouping, graph edges, a text form), the engine compiles each kind once, and adding a screen means adding a class, not an endpoint. Read only; every value is bound as a parameter.

## Superset comparison

**Table 1 — Sources compared**

| Source | What it has | Verdict |
|--------|-------------|---------|
| dotex `OData` and `HttpQueryVerb` designs | Transport and `$filter` style options over `IQueryable<T>` | Transports; this engine can sit behind either |
| dotex `OoBDev.Search`, `Search.Abstractions` | Custom search syntax and ranking, `IVectorStore`, graph models | The text language is one more syntax; vector spaces map to `IVectorStore` only where the store is a SQL table (see below) |
| dotex `VectorSearch` design | Vector store contracts | Spaces here are declared per kind and run inside the kind's SQL |
| Catalog `Storage.Query` core (about 1100 lines, 98 files with kinds and contract) | Attributes `QueryKind`, `Facet`, `Predicate`, `Space`, `Relation`, `Edge`, `Hint`, `SearchOperator`; `KindRegistry`; `QueryCompiler` and `FilterCompiler` (WHERE from declared facets, bound values); `QueryEngine` (rows, facets, buckets, graph, hints, count, text embedding hook); `SearchParser`, `QueryText`; `Describer` and `OpenApiBuilder` (description endpoint and OpenAPI from the registry) | Base for the library |
| Catalog `Cursor` | Offset cursor (`o<offset>`), documented as a placeholder for a keyset cursor | Replaced: the acceptance criterion asks for stable paging, so the library gets a keyset cursor |
| Catalog `Storage.Query.Web` (`/q`, `/parse`, relay, probe cache) | Host and verbs | Becomes a thin host sample; transport choice is the owner's existing HTTP decision |
| Catalog kinds (`FileRow`, `BurstRow`, ...) | Application-specific row classes with PostgreSQL SQL | Stay in the catalog |

## Design

- **Attributes carry SQL written by the application author**, never request text. The engine only splices declared fragments (`Expr`, `Base`, `Joins`, predicate and operator SQL) and binds every request value. A filter on an undeclared facet is a 400 naming the facet.
- **Model package without a database:** attributes, `QueryRequest`, `QueryResult`, the registry, the text parser and the description builders reference no database type.
- **Dialect interface:** `IQueryDialect` (quoting, parameters, limit, text match, distance expressions); the first dialect is PostgreSQL (Npgsql lives only in that adapter). The OData builder or a SQL Server dialect can follow.
- **Keyset cursor:** the cursor encodes the sort values and the key of the last row (signed), so rows that change between pages are neither repeated nor skipped within the sort order.
- **Text form and JSON form** parse to the same `QueryRequest`; a test asserts equal results.
- **Rights:** a kind may require an application right (`[ApplicationRight]` model of the HTTP design) checked before compile; the engine has no rights model of its own.
- **Vector spaces:** declared per kind (table, id and vector columns, metric, optional halfvec cast) and compiled into the kind's SQL; the text anchor uses an injected embedder delegate.
- No source row content is logged; only kind, shape and timing.

## Migration plan

**Table 2 — Projects**

| Project | Current location | Target location | Namespace | Package or unit |
|---------|------------------|-----------------|-----------|-----------------|
| Attributes, model, registry, parser, compiler, describer | `C:\repo\_storage\src\Storage.Query` (read-only reference) | new `src/Framework/OoBDev.Query.Metadata` | `OoBDev.Query.Metadata` | packable, no database reference |
| PostgreSQL dialect and engine runner | `QueryEngine.cs`, `SqlParameters.cs` | new `src/ExternalServices/OoBDev.Query.Postgres` | `OoBDev.Query.Postgres` | packable adapter, Npgsql only here |
| Host sample | `Storage.Query.Web` | after the placement answer (an example under `src/Examples`) | `OoBDev.Example.Query` | sample |
| Tests | `tests/Storage.Query.Tests` | `OoBDev.Query.Metadata.Tests`, `OoBDev.Query.Postgres.Tests` (compile output as `Unit`; database run as `Integration` on the stack's PostgreSQL if one exists) | matching | MSTest |

**Table 3 — References that change**

| Project | Change |
|---------|--------|
| Storage catalog | drops `Storage.Query` and references the packages (outside this repository) |
| Everything else | none |

## Testing strategy

Registry: a class with attributes appears in the description with its facets, predicates and spaces. Compiler: golden SQL for filter, sort, predicate, search operator, negation, near, count; an undeclared facet or non-filterable facet throws a message naming it; no request string appears in the SQL text (asserted by scanning the output for the sent values). Cursor: two pages with a row inserted between them neither repeat nor skip. Text and JSON forms give equal requests. OpenAPI: generated document validates against the registry. Database run: `Integration` when a PostgreSQL service exists in the stack, otherwise recorded as unrun. Target 85 to 90 percent coverage.

## Not in the first slice

- Hints, relations, graph shape and buckets (second slice, after rows, facets and paging).
- Snapshot sources and the anchor store (catalog caching).
- GraphQL and OData transports on top (see the existing transport designs).
