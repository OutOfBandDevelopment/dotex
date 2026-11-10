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

## Planned hybrid contract

Not built. The behaviour recovered from the commented `HybridProvider`, to be implemented as one service in `OoBDev.Search` when a consumer needs it:

1. Query lexical and semantic with `limit × 2`, in parallel.
2. Full outer join on the item key (the path hash).
3. Rescale the lexical score by the maximum lexical score when that maximum is above 1; use 0 when absent. Use the semantic score as is, or 0 when absent.
4. The item's score is the larger of the two; order descending; take `limit`.
5. The result's type is the union of the providers that found it (`Lexical | Semantic`).

Open design points before building: add `SearchTypes Type` to `SearchResultModel`; pass the page through; make the weights configurable (`AddValidatedOptions<T>()`) instead of "larger of"; decide the item key (`ItemId` today).
