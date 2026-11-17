# Feature request: metadata-driven query engine (kinds declared by attributes, one query entry point)

- **Status:** triaged
- **Requested by:** owner (filed from the NAS storage catalog application, `C:\repo\_storage`, `src/Storage.Query`)
- **Date:** 2026-11-11
- **Area:** new library near `OoBDev.AspNetCore.OData`, `OoBDev.Search` and `OoBDev.Data.Vectors`; the HTTP QUERY verb and OData designs

## Need

An application with many read screens ends up with one endpoint per screen. The catalog application replaced about forty read endpoints with one: a *kind* (a row class such as images, files, pages, people) declares by attributes what can be asked of it, and one request shape answers every kind. Filters, sort, paging, similarity search, grouping and a text form all come from the declaration, so adding a screen means adding a class, not an endpoint. The same declaration generates the API reference page and the OpenAPI document, so documentation cannot drift.

dotex has the pieces around this (`OData`, `HttpQueryVerb`, `Search`, `Data.Vectors`) but a search of the requests and designs found no declaration model that is not tied to `IQueryable<T>` or to one search syntax.

## Proposed behavior

A kind is a class carrying attributes; the engine compiles each kind once and answers one request:

```csharp
[QueryKind("files", Base = "select ... from catalog.files f")]
public sealed class FileRow
{
    [Facet("size", Role = FacetRole.Measure)] public long Size { get; init; }
    [Facet("type")] public string Type { get; init; } = "";
    [Predicate("rm", Values = "hide,only", Sql = "...")] ...
    [Space("similar", Metric = SpaceMetric.Cosine, Table = "...", VectorColumn = "embedding")] ...
}
```

```json
POST /q   (or QUERY /q)
{ "kind": "files", "filter": { "type": "pictures" }, "sort": "-size", "top": 50,
  "near": { "space": "similar", "anchor": "<id>" }, "after": "<cursor>" }
```

The answer carries rows, a next cursor and, on request, a count, facet counts, buckets (a grid or histogram) and relations (graph edges). A text form (`type:pictures by size top 50 like <id>`) parses to the same request through `GET /parse`, so a search box and a saved link use the one model.

Pieces to take, in order:
1. Attributes and the registry that compiles them (no database types in the public surface).
2. A compiler behind a dialect interface (the current one emits PostgreSQL; the OData builder could be a second dialect).
3. Keyset cursor paging that stays stable under sort.
4. The text language and its parser.
5. Generation of the reference page per kind and of the OpenAPI document from the registry.

## Acceptance criteria

- Given a class with facet attributes, when the registry loads, then the kind appears in the description endpoint with its facets and predicates and no other code is needed.
- Given a request with a filter on an undeclared facet, when sent, then the answer is a 400 naming the facet; no SQL is built from request text.
- Given two pages requested with `after`, when rows change between the calls, then no row repeats and none is skipped within the sort order.
- Given a similarity request, when the space is a vector column, then rows come back ordered by distance with the distance in the row.
- Given the same request as JSON and as text, when both run, then the results are equal.

## Existing code checked

- `docs/design/ODataQuery` and `docs/design/HttpQueryVerb`: the transport and the `$filter` style options on `IQueryable<T>`; this request is the declaration side that decides what may be asked, and it can sit behind either transport.
- `2026-01-20-query-framework-example.md`: stored-procedure mapping by attributes, a different direction (writes and procedure calls).
- `OoBDev.Search`: the custom search syntax and ranking; not compared in code. The catalog's text language could be one more syntax.
- Source of the engine is `src/Storage.Query` in the catalog application (98 files, with its own test project); the dotex code was not read beyond project names and the design pages.

## Source references (NAS storage catalog application)

Paths are in the catalog repository `C:epo_storage`. They are the code to merge from when this feature is taken in; the catalog will drop its copy and consume the framework version afterward (owner 2026-11-11).

- Engine: `src/Storage.Query/` (`QueryEngine.cs`, `QueryCompiler.cs`, `FilterCompiler.cs`, `Cursor.cs`, `KindRegistry.cs`, `SqlParameters.cs`), attributes in `src/Storage.Query/Metadata/`, kinds in `src/Storage.Query/Kinds/`.
- Text language: `SearchParser.cs`, `SearchOperator.cs`, `QueryText.cs`, `ParsedSearch.cs`; page `docs/features/query-host/query-language.md`.
- Host and verbs: `src/Storage.Query.Web/` (`/q`, `/parse`, `CommandRelay.cs`, `ProbeCache.cs`).
- Tests: `tests/Storage.Query.Tests` (engine, compiler, DocGen of `docs/api-guide/`), `tests/Storage.Query.Web.Tests`.
- Design: `docs/features/query-host.md`, `docs/features/query-host/`.

## Security and operations

Read-only by design: the engine only builds `select` statements from declarations, with every value bound as a parameter. Access control is a hook per kind (a kind can require a right); it should use the application rights design rather than its own. Cost is one compiled plan per kind at start.

## Priority

- **Impact:** removes the endpoint-per-screen pattern for any data-heavy application.
- **Needed by:** no date; the application keeps its own copy until this exists.
- **Workaround today:** the catalog application owns the engine.

## Open questions

- Should the library depend on a relational dialect from the start, or ship the attribute model and a PostgreSQL dialect separately (recommended: model plus a dialect interface, one dialect first)?
- Is `POST /q` with a `QUERY` verb fallback acceptable as the single entry point, or should each kind keep its own route?

## Triage notes

2026-11-12: superset comparison done and the design written: [MetadataQuery](../../design/MetadataQuery/README.md); placement confirmed 2026-11-12.

2026-11-11: triaged. Accepted as a backlog idea from the storage and photo-library review; the code named above was not read in full, so the first step is the superset comparison before any design. Face and pet matching also needs an owner decision on personal data. Backlog: BACKLOG.md, Consolidation requests.
