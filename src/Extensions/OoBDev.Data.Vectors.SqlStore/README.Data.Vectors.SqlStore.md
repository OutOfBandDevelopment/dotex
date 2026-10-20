# OoBDev.Data.Vectors.SqlStore

`IVectorStoreProvider` over the `[embedding].[Vectors]` table of `OoBDev.Data.Vectors.DB`. A collection is a row in `[embedding].[Sources]`, metadata is JSON in `[Metadata]`, and neighbors are ranked in SQL with `VectorF.Distance`.

```csharp
services.TryAddSqlVectorStore(configuration);   // section SqlVectorStoreOptions: ConnectionString, Metric, Limit
var store = factory.Create("notes");
await store.StoreVectorsAsync([embedding], new Dictionary<string, object> { ["title"] = "Hello" });
await foreach (var hit in store.FindNeighborsAsync(query)) { }
```

Vectors of a different length than the query are skipped. The Qdrant adapter registers the same factory interface; register one of them.
