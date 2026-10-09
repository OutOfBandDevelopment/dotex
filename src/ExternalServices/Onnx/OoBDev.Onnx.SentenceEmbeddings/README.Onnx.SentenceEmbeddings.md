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
- Model differences are options: special tokens (`ClsToken`, `SepToken`, `UnkToken`, `PadToken`, `MaskToken`), `TokenTypeIdsName` (null when the model has no token type input), `Prefix`, `Pooling`, `LayerNormalize` (pooled vector is layer normalised before truncation) and `Dimensions`.
- `HuggingFaceHubCache` gives the cache root, snapshot folder and pinned download sources in the Hugging Face hub layout, so model files are shared with Python tools; `ModelFiles` names may include a sub folder such as `onnx/model.onnx`.
- Presets: [all-MiniLM-L6-v2](../../SBert/OoBDev.SBert.AllMiniLmL6V2/README.SBert.AllMiniLmL6V2.md), [all-mpnet-base-v2](../../SBert/OoBDev.SBert.AllMpnetBaseV2/README.SBert.AllMpnetBaseV2.md), [nomic-embed-text-v1.5](../../SBert/OoBDev.SBert.NomicEmbedTextV1_5/README.SBert.NomicEmbedTextV1_5.md).

## Tokenizer

The tokenizer is first-party (`SentenceTokenizer`): the reference BERT basic tokenizer (control and whitespace cleanup, CJK splitting, lower casing, accent stripping, punctuation splitting, literal `[CLS]`/`[SEP]`/`[UNK]`/`[PAD]`/`[MASK]` kept as special tokens) followed by greedy WordPiece. `Microsoft.ML.Tokenizers` was tried and dropped because it does not treat tab/newline as whitespace, drops ASCII symbols such as `_` and `=`, drops unknown characters instead of emitting `[UNK]`, and does not decompose Hangul. Accent stripping uses `string.Normalize`, which needs ICU (or full normalization support) on Linux.

Verified against vectors from the original Hugging Face model: see `scripts/embeddings/README.md`.
