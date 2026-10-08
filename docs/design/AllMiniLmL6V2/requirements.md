# AllMiniLmL6V2 — Requirements

**Status:** Draft · 2026-10-07 · Epic: AI · Related: [README](README.md), [architecture](architecture.md)

[↑ Index](README.md) · [Architecture →](architecture.md)

## Contents

- [Background](#background)
- [Goals and non-goals](#goals-and-non-goals)
- [Requirements](#requirements)
- [Open questions](#open-questions)

## Background

`OoBDev.SBert.AllMiniLML6v2Sharp` wraps `src/ExternalServices/AllMiniLML6v2Sharp`, a fork of an unmaintained third-party library. The model files (`model.onnx`, 90 MB, plus `vocab.txt`, `tokenizer.json`) are a Hugging Face repository checked out as a git submodule under the adapter's `model/` folder.

Observed in the fork (to be confirmed by tests, see [testing strategy](testing-strategy.md)):

**Table 1 — Known weaknesses of the fork**

| # | Observation | Source |
|---|-------------|--------|
| 1 | Owner reports it does not behave correctly under multiple threads | Owner statement; to be reproduced by a stress test |
| 2 | The tokenizer runs up to three times per call (`Tokenize`, then `Encode` tokenizes again) | `AllMiniLmL6V2Embedder.GenerateEmbedding` |
| 3 | Mean pooling and normalisation are hand-built over `DenseTensor` with recursive loops (about 270 lines) | `OnnxExtensions.cs` |
| 4 | `VocabLoader` stops at the first empty line | `VocabLoader.Load` |
| 5 | The batch path takes the attention-mask width from the first row | `GenerateEmbeddings` |
| 6 | `IEmbeddingProvider.Length` blocks on `.Result` and caches in an unsynchronised field | `AllMiniLmL6V2Embedding.Length` |
| 7 | `PercentageOfParallelism` is declared in options but never read | `AllMiniLmL6V2EmbeddingOptions` |
| 8 | Registered with an upper-case key (`ALLMINILM`) rather than a kebab-case constant | `ServiceCollectionExtensions` |
| 9 | `Microsoft.ML` and `libtorch-cpu-win-x64` (Windows-only native) are not needed for ONNX inference | csproj |
| 10 | Model path is resolved relative to the working directory (`./model/model.onnx`) with an assembly-folder fallback | `CachedAllMiniLmL6V2Embedder` |

## Goals and non-goals

**Goals:** same model and same vectors; thread-safe by construction; first-party code that follows the repository patterns; smaller dependency set and cross-platform; platform abstractions (`Microsoft.Extensions.AI`).

**Non-goals:** a different model, GPU execution providers, training or fine-tuning, a general Hugging Face `AutoModel` (tracked in the TODO.md backlog; this design is its first consumer), a vector store.

## Requirements

**Table 2 — Requirements**

| ID | Requirement | Priority |
|----|-------------|----------|
| REQ-001 | Use the same `all-MiniLM-L6-v2` ONNX model file as today; no model conversion in this phase | Must |
| REQ-002 | Produce token ids identical to the fork for a corpus of test sentences (ASCII, accents, CJK, emoji, punctuation, very long, empty) | Must |
| REQ-003 | Produce embeddings whose cosine similarity to the fork's output is at least 0.999 for every corpus sentence (element tolerance documented) | Must |
| REQ-004 | Safe for concurrent use from many threads on one singleton; no shared mutable state other than the thread-safe `InferenceSession` | Must |
| REQ-005 | Results do not depend on batch composition: an input embedded alone and inside a batch gives the same vector within tolerance | Must |
| REQ-006 | Expose `IEmbeddingGenerator<string, Embedding<float>>` and an `IEmbeddingProvider` adapter that reports `Length` (384) without blocking on async code | Must |
| REQ-007 | Register through `TryAdd…Services` and a kebab-case keyed registration constant; selectable through the keyed selection factory | Must |
| REQ-008 | Options for model folder, maximum sequence length (default 256 per the model card), maximum concurrent inferences, intra-op threads; validated at startup | Must |
| REQ-009 | Cancellation honoured before inference starts and between batches | Should |
| REQ-010 | Batches larger than a configured size are split; padding per batch only to the longest sequence | Should |
| REQ-011 | Emit `ActivitySource` and `Meter` telemetry (duration, batch size, token count) | Should |
| REQ-012 | Caching through platform `UseDistributedCache` middleware or the caching proxy rather than a bespoke cached embedder | Should |
| REQ-013 | Model resolved from a folder; optional Hub id or download loader later | Could |
| REQ-014 | No dependency on `Microsoft.ML`, `libtorch-*` or the fork after cutover; runs on Windows, Linux and macOS | Must |
| REQ-015 | The generic runner supports other BERT-family sentence models by configuration (input names, pooling, normalisation), so later presets add no runner code | Could |
| REQ-016 | `README.{Project}.md` per new project; XML docs on public APIs; 80% coverage | Must |

## Open questions

**Table 3 — Open questions**

| # | Question | Default if unanswered |
|---|----------|-----------------------|
| 1 | Keep the model as a git submodule under the preset project, or move to an artifact feed or Hugging Face download (see the TODO.md hosting decision)? | Keep the submodule during the compatibility phase |
| 2 | Is `Microsoft.ML.Tokenizers` `BertTokenizer` output identical to the fork for this vocabulary (casing, accent stripping, CJK)? | Decided by the compatibility suite; fall back to a first-party WordPiece tokenizer if not |
| 3 | Maximum sequence length: the model card says 256 (trained on 128); the fork does not truncate by default | 256, configurable |
| 4 | Keep the old key `ALLMINILM` as an alias for one release? | Yes, alias, then remove |

[↑ Index](README.md) · [Architecture →](architecture.md)
