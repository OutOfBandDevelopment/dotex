# OoBDev.SBert.AllMpnetBaseV2

In-process all-mpnet-base-v2 sentence embeddings (768 dimensions) as an `IEmbeddingProvider` and a
`Microsoft.Extensions.AI` `IEmbeddingGenerator<string, Embedding<float>>`, built on
[OoBDev.Onnx.SentenceEmbeddings](../../Onnx/OoBDev.Onnx.SentenceEmbeddings/README.Onnx.SentenceEmbeddings.md).

```csharp
services.TryAddAllMpnetBaseV2Services(configuration);
var provider = serviceProvider.GetRequiredKeyedService<IEmbeddingProvider>(SBertGlobals.AllMpnetBaseV2Key);
```

- Options bind to the `AllMpnetBaseV2` section as `OnnxSentenceEmbeddingOptions` on top of this model's defaults.
- Registered under the key `all-mpnet-base-v2` (`SBertGlobals.AllMpnetBaseV2Key`). The first model registered in an app is also the default unkeyed provider, so this package can sit beside the other model packages.
- Blank input returns a zero vector of the right length.
- MPNet vocabulary: start `<s>`, end `</s>`, padding `<pad>` (id 1), no token type ids; sequences are cut at 384 tokens like the original.
- Mean pooling and L2 normalisation, 768 dimensions.
- The package does not contain the model (about 420 MB). On first use the generator downloads `onnx/model.onnx` and `vocab.txt` from Hugging Face (`sentence-transformers/all-mpnet-base-v2`, pinned revision, SHA-256 verified) into the Hugging Face hub cache layout. Python tools (`huggingface_hub`, `sentence-transformers`) use the same repository folder, so a machine downloads each model once and every app shares it. The cache root follows the Python library: `HF_HUB_CACHE`, else `HF_HOME/hub`, else `XDG_CACHE_HOME/huggingface/hub`, else `~/.cache/huggingface/hub`.
- In containers set `ModelPath` (or `HF_HUB_CACHE`) to a mapped volume to keep the files between restarts. Several processes can share the folder (a lock file serialises the download).
- Call `AllMpnetBaseV2Model.EnsureAsync(options, logger)` at startup to download ahead of the first request.
- Output was compared with the Hugging Face original on 30+ texts (accents, CJK, emoji, whitespace, long inputs); see `OoBDev.Onnx.SentenceEmbeddings.Tests`.
