# OoBDev.SBert

Embedding provider that calls an SBERT sentence-transformer HTTP service (the `sbert` container of the test stack) and implements `IEmbeddingProvider` from [OoBDev.AI.Abstractions](../../../Framework/OoBDev.AI.Abstractions/README.AI.Abstractions.md). The in-process alternatives are in [OoBDev.Onnx.SentenceEmbeddings](../../Onnx/OoBDev.Onnx.SentenceEmbeddings/README.Onnx.SentenceEmbeddings.md).

## Use

```csharp
services.TryAddSbertServices(configuration);                     // section SentenceEmbeddingOptions
services.TryAddSbertServices(configuration, "MyEmbeddings");     // or a named section
```

```json
{ "SentenceEmbeddingOptions": { "Url": "http://localhost:8000" } }
```

When the section has no `Url`, nothing is registered. Otherwise the call adds:

- `SentenceEmbeddingOptions` (`Url` required, validated by data annotations);
- a typed `HttpClient` for `ISentenceEmbeddingClient` with `Url` as base address;
- `IEmbeddingProvider` (`SentenceEmbeddingProvider`), also keyed `sbert` (`SBertProviderGlobals.ProviderKey`);
- a health check named `sbert`, which calls `/health` and reports `Degraded` with the exception when the service is unreachable.

In `DEBUG` builds the section name is a required argument (rule `coding-standards`: builder parameters are forwarded explicitly).

## Behaviour

The client calls `GET /generate-embedding?query=<text>` and reads the `embedding` array of the JSON response, as `float` (`GetEmbeddingAsync`, used by the provider) or `double` (`GetEmbeddingDoubleAsync`). `SentenceEmbeddingProvider.Length` is found by embedding `"hello world"` once and cached. The query text is URL-escaped. Known limit: the `model` argument of `GenerateEmbeddingAsync` is ignored, because the service runs one fixed model; long texts are better sent through the ONNX provider.

Model presets for this service: [AllMpnetBaseV2](../OoBDev.SBert.AllMpnetBaseV2/README.SBert.AllMpnetBaseV2.md) and [NomicEmbedTextV1_5](../OoBDev.SBert.NomicEmbedTextV1_5/README.SBert.NomicEmbedTextV1_5.md).

## Notes

- https://www.sbert.net/
