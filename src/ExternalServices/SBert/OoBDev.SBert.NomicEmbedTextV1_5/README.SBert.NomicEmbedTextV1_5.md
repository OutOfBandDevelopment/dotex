# OoBDev.SBert.NomicEmbedTextV1_5

In-process nomic-embed-text-v1.5 sentence embeddings (768 dimensions) as an `IEmbeddingProvider` and a
`Microsoft.Extensions.AI` `IEmbeddingGenerator<string, Embedding<float>>`, built on
[OoBDev.Onnx.SentenceEmbeddings](../../Onnx/OoBDev.Onnx.SentenceEmbeddings/README.Onnx.SentenceEmbeddings.md).

```csharp
services.TryAddNomicEmbedTextV1_5Services(configuration);
var provider = serviceProvider.GetRequiredKeyedService<IEmbeddingProvider>(SBertGlobals.NomicEmbedTextV1_5Key);
```

- Options bind to the `NomicEmbedTextV1_5` section as `OnnxSentenceEmbeddingOptions` on top of this model's defaults.
- Registered under the key `nomic-embed-text-v1.5` (`SBertGlobals.NomicEmbedTextV1_5Key`). The first model registered in an app is also the default unkeyed provider, so this package can sit beside the other model packages.
- Blank input returns a zero vector of the right length.
- Nomic models need a task prefix. The default is `search_document: ` (text to be searched); set `Prefix` to `search_query: ` on the side that embeds search questions. Other prefixes are `clustering: ` and `classification: `.
- Matryoshka sizes: set `Dimensions` to 512, 256, 128 or 64. The pooled vector is layer normalised, cut, then scaled to unit length, as the model card describes.
- `MaxSequenceLength` defaults to 512; the model accepts up to 8192 at a higher memory cost.
- The package does not contain the model (about 520 MB). On first use the generator downloads `onnx/model.onnx` and `vocab.txt` from Hugging Face (`nomic-ai/nomic-embed-text-v1.5`, pinned revision, SHA-256 verified) into the Hugging Face hub cache layout. Python tools (`huggingface_hub`, `sentence-transformers`) use the same repository folder, so a machine downloads each model once and every app shares it. The cache root follows the Python library: `HF_HUB_CACHE`, else `HF_HOME/hub`, else `XDG_CACHE_HOME/huggingface/hub`, else `~/.cache/huggingface/hub`.
- In containers set `ModelPath` (or `HF_HUB_CACHE`) to a mapped volume to keep the files between restarts. Several processes can share the folder (a lock file serialises the download).
- Call `NomicEmbedTextV1_5Model.EnsureAsync(options, logger)` at startup to download ahead of the first request.
- Output was compared with the Hugging Face original on 30+ texts (accents, CJK, emoji, whitespace, long inputs); see `OoBDev.Onnx.SentenceEmbeddings.Tests`.
