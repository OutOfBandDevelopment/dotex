# OoBDev.Search

Vector store plumbing for semantic search: a factory that picks the store for a collection and a typed wrapper over it. The contracts (`IVectorStore`, `IVectorStore<T>`, `IVectorStoreProvider`, `IVectorStoreProviderFactory`, `VectorStoreAttribute`) are in `OoBDev.Search.Abstractions`.

Lexical and summary-generation providers are not in this library; the hybrid merge is below ([HybridSearch](../../../docs/design/HybridSearch/README.md)).

## Registration

```csharp
services.TryAddSearchServices();
```

This adds `IVectorStoreFactory` (`VectorStoreFactory`) and the open generic `IVectorStore<T>` (`WrappedVectorStore<T>`), both transient. A store provider (for example the SQL store in `OoBDev.Data.Vectors.SqlStore`) must be registered separately.

## Choosing a store

`VectorStoreFactory.Create(collectionName)` resolves the provider in this order and sets its `CollectionName`:

1. a keyed `IVectorStoreProvider` registered under the collection name;
2. the first non-null result of the registered `IVectorStoreProviderFactory` services;
3. the unkeyed `IVectorStoreProvider` (required; resolving fails when none is registered).

`Create<T>()` names the collection from `[VectorStore]` on `T`, else `[Table]`, else the type name.

## Typed store

`IVectorStore<T>` wraps the store for `T`'s collection and forwards `FindNeighborsAsync` (with or without a group-by), `ListAsync` and both `StoreVectorsAsync` overloads.

## Embedding version tag

`EmbeddingVersion` records which model produced each vector, inside the vector metadata (key `embeddingModel`), so no store needs a schema change:

- `Tag(modelId, dimensions)` builds the tag, for example `all-MiniLM-L6-v2/384`; `Stamp(metadata, tag)` returns a copy of the metadata carrying it (pass the result to `StoreVectorsAsync`).
- `Read(result)`, `IsStale(result, currentTag)` and `StaleIds(results, currentTag)` find the rows to re-embed after a model change; a vector stored before tagging counts as stale.

`ReembedAsync(store, currentTag, embed)` replaces the stale rows: the callback returns a new vector for a stale result (it knows the source text; null skips the row), the new row is stored with the current tag and then the old id is removed with `IVectorStore.DeleteAsync` (store first, so a failure never loses a row).

`IVectorStore.DeleteAsync(ids)` is implemented by the in-memory example store, SQL Server (`VectorID` in the collection), Qdrant (point ids; `StoreVectorsAsync` now returns them), Azure AI Search and pgvector (the item id removes all its vectors). An unknown id is ignored.

The embedder presets expose the model id and dimensions through `EmbeddingGeneratorMetadata`, which is what to feed `Tag`.

## Relevance metrics

`RelevanceMetrics` scores a ranked id list against the ids marked relevant: `RecallAtK`, `ReciprocalRank` and `MeanReciprocalRank`. Keep a small set of queries with expected hits beside a search feature and assert a minimum score in a test, so a change to fusion, filters or an embedding model that hurts ranking fails the build. The set itself is project data, not part of this library.

## Search requests, text form and hybrid merge

One request model (`SearchRequest`, in `OoBDev.Search.Abstractions`) is understood by every `ISearchProvider`: free text, `FieldFilter` conditions, an optional `SearchAnchor` (`like` an item or `near` a text or vector), sort, page size and an opaque cursor. Each `SearchItem` carries a `SearchExplanation` (field, vector index, distance, the score per source and the fusion name). A provider lists the filters it supports in `SupportedFilters` and throws `UnsupportedFilterException` (filter name only, never the value) for the rest.

- `SearchTextParser.Parse(text)` builds a request from `ext:pdf -tag:draft size:>=10 name:~logo tag:"two words" like:item-1 near:beach sort:-date,name top:5 after:<cursor> plain words`. `-` negates, `>`, `>=`, `<`, `<=` and `~` (contains) prefix a value, quotes group words, and `like`, `near`, `sort`, `top` and `after` are reserved names.
- `IScoreFusion` combines ranked lists by item id: `MaxNormalisedFusion` (each source divided by its largest score, the item keeps the largest) and `ReciprocalRankFusion` (`1/(k+rank)` summed, default `k` 60). The choice is `Search:Fusion:Strategy` (`max-normalised` or `reciprocal-rank`) and `Search:Fusion:K`, registered with `TryAddSearchFusion(configuration)`.
- `HybridSearchProvider(sources, fusion)` runs the request against every source and fuses the results. It supports the filters every source supports, returns one page and rejects a cursor.

Not built yet: the keyword (OpenSearch) and vector providers, the SQL-kind provider over the metadata query engine and the filter-parity Integration set ([design](../../../docs/design/SearchMerge/README.md)).

## Vector search provider

`VectorSearchProvider(IVectorStore, IEmbeddingProvider)` is an `ISearchProvider` over nearest neighbours: the query vector is `Anchor.NearVector`, else the embedding of `Anchor.NearText` or the request text. It supports no filters (a store holds none), no cursor and no like-item anchor, and an empty query returns no hits. Use it as a source of `HybridSearchProvider`.

## Access filtering

`AccessFilteredSearchProvider(inner, IItemAccessPolicy, overFetch = 3)` wraps any `ISearchProvider` (keyword, vector, hybrid) and drops the hits the caller may not read, so search never reveals a document the caller could not open. The policy (`IItemAccessPolicy.FilterReadableAsync`, batch by item id) is the application's: for example it maps item ids to `[ApplicationRight]` checks or to the owning record's access rule. The inner provider is asked for `PageSize * overFetch` hits so a page can still fill; the result is one page with no cursor, like the hybrid provider. Counts or facets from the inner provider are not filtered, so do not expose them without a policy-aware source.
