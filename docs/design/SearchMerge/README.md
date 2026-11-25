# Search merge: one request model over keyword, vector, hybrid and SQL kinds

Status: **first slice built 2026-11-16** (request model, text parser, max-normalised and reciprocal-rank fusion, hybrid provider; the vector provider was added the same day; the keyword provider (OpenSearch) was added the same day; the SQL-kind provider (PostgreSQL query adapter) and the filter-parity Integration set were added the same day). Design written 2026-11-12. It builds on the confirmed placements of MetadataQuery and EmbeddingClientAndPgVector. Requests: [search merge and hybrid](../../features/completed/2026-11-11-search-merge-kinds-hybrid-and-vectors.md), [superset review](../../features/completed/2026-11-11-superset-review-of-overlapping-features.md). Related: [HybridSearch](../HybridSearch/README.md), [MetadataQuery](../MetadataQuery/README.md), [EmbeddingClientAndPgVector](../EmbeddingClientAndPgVector/README.md).

## Contents

1. [Need](#need)
2. [Superset comparison](#superset-comparison)
3. [Design](#design)
4. [Superset review index](#superset-review-index)
5. [Migration plan](#migration-plan)
6. [Testing strategy](#testing-strategy)
7. [Not in the first slice](#not-in-the-first-slice)

## Need

One search concept with a request any provider understands (free text, field filters, `like` and `near` anchors, sort and cursor) and a result that says why it matched.

## Superset comparison

**Table 1 — Sources compared**

| Source | What it has | Verdict |
|--------|-------------|---------|
| dotex `OoBDev.Search` and `Search.Abstractions` | Providers (OpenSearch lexical, Qdrant semantic), `IVectorStore`, `SearchResultModel`, group-by neighbours | Base for providers; no shared request model beyond a query string |
| dotex HybridSearch planned contract | Outer join on item key, lexical rescaled by its maximum, larger score wins, result type is the union | Documented, unbuilt; one fusion among several |
| Catalog `SearchParser` (35 lines) | Tokens into operators (`ext:pdf`, `-tag:x`, `size:>10mb`) and plain words; plain words match by meaning where a kind has a text space | Becomes the text form of the shared request |
| Catalog spaces | Metric, many vectors per item, "why it matched" note | Becomes the explanation (see EmbeddingClientAndPgVector) |
| Catalog keyset-paging intent | Offset cursor today | Keyset cursor from the MetadataQuery design |

## Design

- **`SearchRequest`:** free text, a list of `FieldFilter(name, comparator, value, negated)`, optional anchors (`like` an item id, `near` a vector or text), sort, page size and an opaque cursor. The text form parses to it with the catalog parser rules (operators plus words); the JSON form is the same record.
- **`ISearchProvider.SearchAsync(request)`** returns `SearchResult { Items, NextCursor }`; each item carries `Explanation { Field, VectorIndex, Distance, Scores }`. A provider declares the filters it supports and rejects the rest by name.
- **Providers:** keyword (OpenSearch), vector (single and multi vector), hybrid, and a SQL-kind provider that adapts the MetadataQuery engine.
- **Hybrid fusion is a strategy,** not a constant: `IScoreFusion` with *max of normalised scores* (the documented behaviour) and *reciprocal rank fusion* (`1/(k+rank)`, k default 60); the result lists both source scores and the fusion name. Choice by options (`AddValidatedOptions<T>()`).
- **Filter parity:** a shared filter test set runs against the keyword and SQL providers on equal data.
- Search reads only; per-kind rights apply as in MetadataQuery.

## Superset review index

The review is done per area in the design that covers it:

**Table 2 — Areas and where the comparison lives**

| Area | Comparison |
|------|------------|
| Query layer, cost limits | [MetadataQuery](../MetadataQuery/README.md) (the dotex `ODataQueryLimits` cost guard is built there: page size, maximum top, filter count, values per filter and text length) |
| Vectors, embeddings | [EmbeddingClientAndPgVector](../EmbeddingClientAndPgVector/README.md) |
| Search | this page |
| Queue | [WorkQueue](../WorkQueue/README.md) |
| Documents and Markdown | [DocumentToMarkdown](../DocumentToMarkdown/README.md) |
| Images (burst, RAW) | [BurstRanking](../BurstRanking/README.md), [RawDevelopment](../RawDevelopment/README.md) |
| Caching | `OoBDev.Caching` (providers, factories, managers) against the catalog `ProbeCache` (19 lines: remember a probe answer for a lifetime, with `TimeProvider`). Nothing missing in the framework: the probe cache is one use of a short-lived keyed cache and moves to the catalog's own use of `OoBDev.Caching`; no capability to add |
| Telemetry | `OoBDev.Telemetry` (config-gated OpenTelemetry, OTLP, correlation and user processors) against catalog `ServiceDefaults` (OpenTelemetry logs, traces, metrics, OTLP, plus Aspire service discovery and `AddStandardResilienceHandler`). Framework covers the telemetry; service discovery and the standard resilience handler (Polly based) are not adopted because the owner avoids Polly (rejected-dependencies memory), so the catalog keeps those two lines or drops them |
| Rights and command relay | ApplicationRights (role to right claims, one checker for every endpoint) is endpoint authorization. The catalog's `Access*` is a different thing: per-record visibility (private, groups, family, lan, public) inherited by folder prefix and by person, which ApplicationRights does not express. `CommandRelay` (428 lines) maps named verbs to a legacy API and goes away with that API. Gap: a data-visibility label model is not in the framework; it is application data in the catalog and is recorded as a backlog candidate, not a framework feature, until a second consumer exists |

## Migration plan

**Table 3 — Projects**

| Project | Current location | Target location | Namespace | Package or unit |
|---------|------------------|-----------------|-----------|-----------------|
| Request model, parser, fusion strategies | `Storage.Query/SearchParser.cs` and `OoBDev.Search` | `src/Framework/OoBDev.Search` and `OoBDev.Search.Abstractions` (extended; breaking changes allowed, unreleased) | `OoBDev.Search` | existing packages |
| SQL-kind provider | none | with the MetadataQuery adapter, after the MetadataQuery adapter exists | `OoBDev.Query.Postgres` | adapter |

## Testing strategy

Unit: parser cases from `tests/Storage.Query.Tests`; RRF and max fusion golden scores; both source scores present in a hybrid result; the best vector's index appears in the explanation. Shared filter-parity set across providers (the SQL provider as `Integration`). Target 85 to 90 percent coverage.

## Not in the first slice

- Built 2026-11-25: the hybrid provider pages with a stateless offset cursor (base64 `hybrid:<offset>`), re-querying each source for `offset + page size + 1` hits and re-fusing, capped by `maxDepth` (default 200). Stable only while the sources are; a foreign cursor throws `ArgumentException`.
- Image variant and learned rerankers: with the first consumer (listed in BACKLOG).
- Built 2026-11-16 in `OoBDev.Search` and `OoBDev.Search.Abstractions`: `SearchRequest`, `ISearchProvider`, `SearchTextParser`, `IScoreFusion` (max-normalised and reciprocal rank), `HybridSearchProvider`, `Search:Fusion` options. `VectorSearchProvider` (over `IVectorStore` and `IEmbeddingProvider`; refuses filters, cursors and like-item anchors, since the store has no filterable fields) built 2026-11-16. `KeywordSearchProvider` in `OoBDev.OpenSearch` built 2026-11-16 (simple query string, term, match and range clauses, offset cursor). `SqlKindSearchProvider` in `OoBDev.PostgreSql.Query` built 2026-11-16 (facet operators over the MetadataQuery engine, keyset cursor). The shared filter-parity set (`src/Tools/OoBDev.Search.Parity.Tests`, Integration: equals, negated equals, range, whole-word text and combined cases on equal data in OpenSearch and PostgreSQL) was built and run 2026-11-16. Known limit: contains is a whole-word match in OpenSearch and a substring match in SQL, so the set uses whole words. The hybrid provider returns one page without a cursor.
