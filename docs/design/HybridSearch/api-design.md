# Hybrid Search — API Design

[← Architecture](architecture.md) · [↑ Overview](README.md) · [Testing strategy →](testing-strategy.md)

**Status:** Draft (from the code as it stands) · **Date:** 2026-11-10

## Contents

- [Built contracts](#built-contracts)
- [Registration](#registration)
- [Planned hybrid contract](#planned-hybrid-contract)

### List of Tables

1. [Table 1 — Vector store members](#table-1)
2. [Table 2 — Result model](#table-2)

## Built contracts

<a id="table-1"></a>**Table 1 — Vector store members**

| Member | Meaning |
|--------|---------|
| `Task<string[]> StoreVectorsAsync(IEnumerable<ReadOnlyMemory<float>> embeddings, IDictionary<string, object> metadata)` | Store the vectors with the shared metadata; returns the new ids |
| `Task<string[]> StoreVectorsAsync(IEnumerable<(ReadOnlyMemory<float> embedding, IDictionary<string, object> metadata)> items, IDictionary<string, object> metadata)` | Store vectors that each carry their own metadata, plus shared metadata |
| `IAsyncEnumerable<SearchResultModel> ListAsync()` | List stored items |
| `IAsyncEnumerable<SearchResultModel> FindNeighborsAsync(ReadOnlyMemory<float> find)` | Nearest neighbours of a vector |
| `IAsyncEnumerable<SearchResultModel> FindNeighborsAsync(ReadOnlyMemory<float> find, string groupBy)` | Nearest neighbours grouped by a metadata key (one result per group) |

`IVectorStore<T>` adds nothing; the type parameter selects the collection (`[VectorStore(CollectionName = ...)]`, then `[Table]`, then the type name). `IVectorStoreFactory` has `Create(string collectionName)` and `Create<T>()`. `IVectorStoreProvider` is an `IVectorStore` with a settable `CollectionName`; `IVectorStoreProviderFactory.Create(collectionName)` returns a provider or null.

<a id="table-2"></a>**Table 2 — Result model**

| Property | Type | Notes |
|----------|------|-------|
| `Score` | `float` | Required; meaning depends on the provider (distance-derived for Qdrant) |
| `ItemId` | `string` | Required |
| `MetaData` | `IDictionary<string, object>?` | Provider payload |

`SearchTypes` is a `[Flags]` enum: `None = 0`, `Semantic = 1`, `Lexical = 2`, `Hybrid = 3` (the union). Nothing on the result model uses it yet.

## Registration

`services.TryAddSearchServices()` adds `IVectorStoreFactory` and the open-generic `IVectorStore<>` as transient. Adapters add the provider (`OoBDev.Qdrant` `ServiceCollectionExtensions`) and read their options through `IOptions<T>`.

## Hybrid contract (built 2026-11-16)

`HybridSearchProvider` in `OoBDev.Search` is an `ISearchProvider` over named source providers (for example keyword and vector) and an `IScoreFusion`:

1. Every source runs in parallel with the same request.
2. `SupportedFilters` is the intersection of the sources; an unsupported filter throws `UnsupportedFilterException` before any source is called.
3. The rankings are fused by `IScoreFusion` (`MaxNormalisedFusion`, the recovered "larger score" rule, or `ReciprocalRankFusion`), chosen by `Search:Fusion:Strategy` through `TryAddSearchFusion`; the page size is taken from the fused list.
4. Result is one page: a cursor is rejected with `NotSupportedException` and `NextCursor` is always null.

Still open: `SearchTypes Type` on the result model, and a paging scheme for fused results. No consumer composes the provider in DI yet; the sources are passed to its constructor.
