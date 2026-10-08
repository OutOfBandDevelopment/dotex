# OoBDev.SBert.AllMiniLmL6V2

In-process all-MiniLM-L6-v2 sentence embeddings (384 dimensions) as an `IEmbeddingProvider`, built on
[OoBDev.Onnx.SentenceEmbeddings](../../Onnx/OoBDev.Onnx.SentenceEmbeddings/README.Onnx.SentenceEmbeddings.md).
It replaces `OoBDev.SBert.AllMiniLML6v2Sharp` and the `AllMiniLML6v2Sharp` fork.

```csharp
services.TryAddAllMiniLmL6V2Services(configuration);
```

- Options bind to the `AllMiniLmL6V2` section as `OnnxSentenceEmbeddingOptions` (the model folder defaults to `model` next to the app).
- Keyed providers: `all-minilm-l6-v2` (`SBertGlobals.AllMiniLmL6V2Key`) and `ALLMINILM` (legacy, kept for one release).
- Blank input returns a zero vector of the right length; the old provider returned an empty vector.
- `Length` comes from the model and never blocks.
- Model files are still copied from the `OoBDev.SBert.AllMiniLML6v2Sharp/model` submodule until the Hugging Face download replaces it.
