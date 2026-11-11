# OoBDev.Qdrant

Qdrant adapter for the vector store contracts (`IVectorStoreProvider`, `IVectorStoreProviderFactory`) over the Qdrant gRPC client. It stores embeddings with metadata as points, lists them and finds nearest neighbours by cosine distance.

## Use

```csharp
services.TryAddQdrantServices(configuration);          // section "QdrantOptions"
var store = provider.GetRequiredService<IVectorStoreProviderFactory>().Create("my-collection");
```

```json
{ "QdrantOptions": { "Url": "http://localhost:6334", "CollectionName": "docs", "EnsureCollectionExists": true } }
```

**Table 1 — Options (`QdrantOptions`, validated with data annotations)**

| Key | Meaning |
|-----|---------|
| `Url` | Required; the Qdrant gRPC address |
| `CollectionName` | Required; the default collection (the factory's `Create(containerName)` argument selects the collection actually used by a provider) |
| `EnsureCollectionExists` | When true, `StoreVectorsAsync` creates the collection (cosine distance, the length of the first vector) if it is missing |

`TryAddQdrantServices` registers the transient `IQdrantGrpcClientFactory` and `QdrantGrpcClient`, `IPointStructFactory`, and the `IVectorStoreProviderFactory`. The factory is added with `AddTransient`, so it does not replace an earlier vector store factory.

## Components

**Table 2 — Types**

| Type | Role |
|------|------|
| `QdrantVectorStoreProvider` | `StoreVectorsAsync` upserts points with a new GUID id and the metadata as payload (string, numbers and value types; other types throw `NotSupportedException`) and returns an empty array because Qdrant ids are not read back. `ListAsync` scrolls up to 1000 points. `FindNeighborsAsync(vector)` queries the nearest 1000; the overload with `groupBy` returns one hit per group value. |
| `QdrantGrpcClientExtensions.EnsureCollectionExistsAsync` | Creates a collection if missing (default size 4096, cosine) and logs it. |
| `PointStructFactory` | Builds points: `CreateFileChunk` (path, hash, chunk and file details as payload), `CreateServiceReference` (type names and description, `type` = `service-instance`) and `CreateQuestion`. |
| `QdrantGrpcClientFactory` | Creates the client with `QdrantChannel.ForAddress(Url)`. |

## Tests

`QdrantVectorStoreProviderTests` are `Integration` tests (nearest vector first, group-by) against the Qdrant container of the test stack. The gRPC client tests in `QdrantGrpcClientTests.cs` are commented out.

Known rough edges: the 1000 result limit and the cosine distance are fixed, the collection-creation code is duplicated in the provider and the extension (both marked TODO), and `CollectionName` in the options is not used by the provider.
