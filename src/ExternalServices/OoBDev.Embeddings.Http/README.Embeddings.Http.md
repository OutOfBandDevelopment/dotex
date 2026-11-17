# OoBDev.Embeddings.Http

`IEmbeddingProvider` over any server that speaks the OpenAI `POST /v1/embeddings` shape (Ollama, llama.cpp, vLLM, TEI, a container). Design: [docs/design/EmbeddingClientAndPgVector](../../../docs/design/EmbeddingClientAndPgVector/README.md). The in-process ONNX providers implement the same interface, so configuration chooses.

## Use

```csharp
services.TryAddHttpEmbeddings(configuration);                 // Embeddings:Http:*
var provider = sp.GetRequiredService<IEmbeddingProvider>();   // or keyed "http-embeddings"
var one = await provider.GenerateEmbeddingAsync("text", null);
var many = await ((OpenAiCompatibleEmbeddingProvider)provider).GenerateEmbeddingsAsync(texts);   // batches of BatchSize, input order
```

## Rules

- Vectors come back in request order (the server's `index` is honoured) and must have the configured `Dimensions`; a wrong count or length is an `EmbeddingServerException`.
- A non-success status becomes `EmbeddingServerException` carrying the status code only; input text and the response body are never put in a message or log.
- The API key is sent as a bearer token and is never logged.

## Options (`Embeddings:Http`)

| Key | Default | Meaning |
|-----|---------|---------|
| `BaseUrl` | required | Server root |
| `Model` | required | Model name sent with each request |
| `Dimensions` | required (1 to 65536) | Expected vector length; `IEmbeddingProvider.Length` |
| `TimeoutSeconds` | 60 | Request timeout |
| `BatchSize` | 32 | Inputs per request |
| `ApiKey` | none | Bearer token |

## Not in the first slice

An image variant (`IImageEmbeddingProvider` posting bytes) and the published container image; see the design.

## Tests

`OoBDev.Embeddings.Http.Tests` (Unit, fake HTTP handler): request shape, model override, order restoration, batch splitting, empty input, error mapping without content, count and dimension checks, options validation and registration.
