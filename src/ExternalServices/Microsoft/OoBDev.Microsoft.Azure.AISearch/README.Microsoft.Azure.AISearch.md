# OoBDev.Microsoft.Azure.AISearch

Azure AI Search adapter behind the Search abstractions: an `IVectorStoreProvider` (and factory) that stores embeddings in an Azure AI Search index and finds nearest neighbors with vector queries. It is the cloud counterpart of the Qdrant adapter ([README](../../Qdrant/OoBDev.Qdrant/README.Qdrant.md)); the design is [HybridSearch](../../../../docs/design/HybridSearch/README.md).

## Registration

```csharp
services.TryAddAzureAISearchServices(configuration); // section "AzureAISearchOptions"
var store = provider.GetRequiredService<IVectorStoreProviderFactory>().Create("documents");
```

| Option | Meaning |
|--------|---------|
| `Endpoint` (required) | Service URL, `https://name.search.windows.net` |
| `IndexName` (required) | Default index |
| `ApiKey` | Admin key; empty means `DefaultAzureCredential` |
| `EnsureIndexExists` | Creates or updates the index from the first vector length (HNSW, profile `oobdev-profile`) |
| `Top` | Neighbors returned (default 50) |

## Index shape

`Id` (key), `Payload` (the metadata as JSON, so any metadata shape round-trips) and `Vector` (`Collection(Single)`, searchable). `FindNeighborsAsync(vector, groupBy)` keeps the best hit per metadata value client-side.

## Tests

- Unit: mapper, provider (mocked `SearchClient`/`SearchIndexClient`) and registration.
- LiveIntegration (manual, may cost money): `AzureAISearchLiveTests` creates and deletes its own index; set `AZURE_SEARCH_ENDPOINT` and `AZURE_SEARCH_API_KEY` (parameters or environment variables). Without them it ends inconclusive.
