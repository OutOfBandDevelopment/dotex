# OoBDev.SBert.AllMiniLmL6V2

In-process all-MiniLM-L6-v2 sentence embeddings (384 dimensions) as an `IEmbeddingProvider`, built on
[OoBDev.Onnx.SentenceEmbeddings](../../Onnx/OoBDev.Onnx.SentenceEmbeddings/README.Onnx.SentenceEmbeddings.md).
It replaces `OoBDev.SBert.AllMiniLML6v2Sharp` and the `AllMiniLML6v2Sharp` fork.

```csharp
services.TryAddAllMiniLmL6V2Services(configuration);
```

- Options bind to the `AllMiniLmL6V2` section as `OnnxSentenceEmbeddingOptions`.
- Keyed providers: `all-minilm-l6-v2` (`SBertGlobals.AllMiniLmL6V2Key`) and `ALLMINILM` (legacy, kept for one release).
- Blank input returns a zero vector of the right length; the old provider returned an empty vector.
- `Length` comes from the model and never blocks.
- The package does not contain the model (about 90 MB). On first use the generator downloads `model.onnx` and `vocab.txt` from Hugging Face (`onnx-models/all-MiniLM-L6-v2-onnx`, pinned revision, SHA-256 verified) into the Hugging Face hub cache layout, so Python tools and other apps share the files. The cache root follows the Python library: `HF_HUB_CACHE`, else `HF_HOME/hub`, else `XDG_CACHE_HOME/huggingface/hub`, else `~/.cache/huggingface/hub`.
- In containers set `ModelPath` (or `HF_HUB_CACHE`) to a mapped volume to keep the files between restarts. Several processes can share the folder (a lock file serialises the download).
- Call `AllMiniLmL6V2Model.EnsureAsync(options, logger)` at startup to download ahead of the first request.
