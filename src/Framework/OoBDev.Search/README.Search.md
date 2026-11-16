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

## Search requests, text form and hybrid merge

One request model (`SearchRequest`, in `OoBDev.Search.Abstractions`) is understood by every `ISearchProvider`: free text, `FieldFilter` conditions, an optional `SearchAnchor` (`like` an item or `near` a text or vector), sort, page size and an opaque cursor. Each `SearchItem` carries a `SearchExplanation` (field, vector index, distance, the score per source and the fusion name). A provider lists the filters it supports in `SupportedFilters` and throws `UnsupportedFilterException` (filter name only, never the value) for the rest.

- `SearchTextParser.Parse(text)` builds a request from `ext:pdf -tag:draft size:>=10 name:~logo tag:"two words" like:item-1 near:beach sort:-date,name top:5 after:<cursor> plain words`. `-` negates, `>`, `>=`, `<`, `<=` and `~` (contains) prefix a value, quotes group words, and `like`, `near`, `sort`, `top` and `after` are reserved names.
- `IScoreFusion` combines ranked lists by item id: `MaxNormalisedFusion` (each source divided by its largest score, the item keeps the largest) and `ReciprocalRankFusion` (`1/(k+rank)` summed, default `k` 60). The choice is `Search:Fusion:Strategy` (`max-normalised` or `reciprocal-rank`) and `Search:Fusion:K`, registered with `TryAddSearchFusion(configuration)`.
- `HybridSearchProvider(sources, fusion)` runs the request against every source and fuses the results. It supports the filters every source supports, returns one page and rejects a cursor.

Not built yet: the keyword (OpenSearch) and vector providers, the SQL-kind provider over the metadata query engine and the filter-parity Integration set ([design](../../../docs/design/SearchMerge/README.md)).

## Vector search provider

`VectorSearchProvider(IVectorStore, IEmbeddingProvider)` is an `ISearchProvider` over nearest neighbours: the query vector is `Anchor.NearVector`, else the embedding of `Anchor.NearText` or the request text. It supports no filters (a store holds none), no cursor and no like-item anchor, and an empty query returns no hits. Use it as a source of `HybridSearchProvider`.
