# OoBDev.Onnx.SentenceEmbeddings

In-process sentence embeddings for BERT-family ONNX models. Exposes `IEmbeddingGenerator<string, Embedding<float>>` from `Microsoft.Extensions.AI`.

Design: [docs/design/AllMiniLmL6V2](../../../../docs/design/AllMiniLmL6V2/README.md).

## Usage

```csharp
var options = Options.Create(new OnnxSentenceEmbeddingOptions { ModelPath = "model" });
using var generator = new OnnxSentenceEmbeddingGenerator(options, NullLogger<OnnxSentenceEmbeddingGenerator>.Instance);

var embeddings = await generator.GenerateAsync(["first sentence", "second sentence"], cancellationToken: ct);
ReadOnlyMemory<float> vector = embeddings[0].Vector; // unit length by default
```

`ModelPath` is a folder with `model.onnx` and `vocab.txt`; relative paths resolve against the application base directory.

## Behaviour

- Thread-safe: one instance can be shared. The `InferenceSession` and tokenizer are read-only, every call allocates its own buffers, and `MaxConcurrentInferences` bounds simultaneous runs.
- Each value is tokenized once, truncated to `MaxSequenceLength`, batched by `MaxBatchSize`, and padded only to the longest row of its batch.
- Pooling is a masked mean (or the first token with `Pooling = Cls`) followed by L2 normalisation.
- Blank input returns a zero vector of the right length instead of an empty one.
- `Dimensions` truncates and re-normalises; it is accepted only when `SupportsDimensionTruncation` is true (Matryoshka-trained models).
