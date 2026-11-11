# OoBDev.Search

Vector store plumbing for semantic search: a factory that picks the store for a collection and a typed wrapper over it. The contracts (`IVectorStore`, `IVectorStore<T>`, `IVectorStoreProvider`, `IVectorStoreProviderFactory`, `VectorStoreAttribute`) are in `OoBDev.Search.Abstractions`.

Lexical, hybrid (merging lexical and vector results) and summary-generation providers are not in this library; the hybrid merge is a backlog item ([HybridSearch](../../../docs/design/HybridSearch/README.md)).

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
