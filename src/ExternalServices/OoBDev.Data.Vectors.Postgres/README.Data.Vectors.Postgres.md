# OoBDev.Data.Vectors.Postgres

`IVectorStore` on PostgreSQL with pgvector. An item owns one or more vectors (one per page, for example); a search returns the best vector of each item. Design: [docs/design/EmbeddingClientAndPgVector](../../../docs/design/EmbeddingClientAndPgVector/README.md). Npgsql and Pgvector are referenced here only (owner, 2026-11-12).

## Use

```csharp
services.TryAddPgVectorStore(configuration);                 // Vectors:Postgres:*; IVectorStore, keyed "pgvector"
var store = sp.GetRequiredService<PgVectorStore>();
await store.EnsureCreatedAsync();                             // extension, table, item index (never the HNSW index)
await store.CreateHnswIndexAsync();                           // explicit, repeatable, slow on big tables

var id = (await store.StoreVectorsAsync(pageVectors, metadata))[0];            // ONE item, many vectors
var ids = await store.StoreVectorsAsync(itemsWithMetadata, sharedMetadata);    // one item per entry, metadata merged

await foreach (var hit in store.FindNeighborsAsync(query)) { }                 // best vector per item
await foreach (var hit in store.FindNeighborsAsync(query, "album")) { }        // best per metadata value
await foreach (var m in store.FindNeighborsWithExplanationAsync(query)) { }    // m.VectorIndex, m.Distance
```

## Rules

- Statements hold only identifiers validated by pattern (`Schema`, `Table`) and every value is a parameter. Vectors of the wrong length are refused before any write.
- Cosine scores are the similarity (`1 - distance`); L2 scores are `1 / (1 + distance)`. Higher is nearer in both.
- Metadata is `jsonb`; values read back are `JsonElement`.
- The HNSW index is never built implicitly. `UseHalfVector` stores `halfvec` and uses the matching operator class.

## Options (`Vectors:Postgres`)

| Key | Default | Meaning |
|-----|---------|---------|
| `ConnectionString` | required | Database with the `vector` extension available |
| `Schema`, `Table` | `public`, `vectors` | Where the vectors live |
| `Dimensions` | required (1 to 2000) | Vector length |
| `Metric` | `Cosine` | `Cosine` or `L2` |
| `UseHalfVector` | false | 16-bit floats |
| `DefaultLimit` | 10 | Neighbours per search |
| `CommandTimeoutSeconds` | 30 | Command timeout |

## Tests

Unit (statements, operator classes, score, identifier validation, registration) and Integration on the `pgvector` service in a throwaway schema: cosine and L2 order, multi-vector item returned once with the matching vector index, group by, halfvec, dimension check, list, metadata merge, explicit HNSW index, and 10,000 vectors with HNSW (top 5 in 10 ms on AGIMUS, 2026-11-16).
