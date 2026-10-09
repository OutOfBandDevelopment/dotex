# AllMiniLmL6V2 — In-Process Sentence Embeddings

**Status:** Implemented and fork removed · 2026-10-08 (design drafted 2026-10-07) · Epic: AI (patterns discovery follow-up) · Related: [TODO.md backlog item](../../../TODO.md), [Extensions.AI spike](../../patterns-discovery/08-spikes/01-extensions-ai.md), [design standard](../../patterns-discovery/06-design-document-standard.md)

Plan to replace the unmaintained fork `src/ExternalServices/AllMiniLML6v2Sharp` (a fork of someone else's work, about 1,250 lines, with `Microsoft.ML` and `libtorch-cpu-win-x64` dependencies) with a first-party, thread-safe implementation that runs **the same `all-MiniLM-L6-v2` ONNX model** under the covers. Cutover happens only after compatibility tests prove that the new code produces the same token ids and (within tolerance) the same embeddings as the fork.

## Documents

**Table 1 — Document set**

| Document | Purpose |
|----------|---------|
| [requirements.md](requirements.md) | Problem, requirements (REQ-001…), non-goals, open questions |
| [architecture.md](architecture.md) | Project names and layer placement, components, flows, alternatives, cutover plan |
| [api-design.md](api-design.md) | Public contracts, options, DI registration, configuration |
| [testing-strategy.md](testing-strategy.md) | Compatibility (golden) tests, concurrency tests, coverage plan |

## Decisions in one view

**Table 2 — Summary of the plan**

| Topic | Decision (proposed) |
|-------|---------------------|
| Model | Unchanged: `sentence-transformers/all-MiniLM-L6-v2` ONNX (`model.onnx`, `vocab.txt`), 384 dimensions |
| Runtime | `Microsoft.ML.OnnxRuntime` only; drop `Microsoft.ML` and `libtorch-cpu-win-x64` |
| Tokenizer | First-party BERT basic tokenizer and WordPiece (`SentenceTokenizer`), verified against vectors from the original Hugging Face model; `Microsoft.ML.Tokenizers` was tried and dropped because it differs from the reference (tab and newline handling, ASCII symbols, emoji, Hangul) |
| Public contract | `IEmbeddingGenerator<string, Embedding<float>>` from `Microsoft.Extensions.AI`, plus the existing `IEmbeddingProvider` adapter for callers that need `Length` |
| Projects | `OoBDev.Onnx.SentenceEmbeddings` (generic runner) and `OoBDev.SBert.AllMiniLmL6V2` (model preset), each with `.Tests` |
| Provider key | `all-minilm-l6-v2` (kebab-case constant in `SBertGlobals`), replacing the upper-case `ALLMINILM` key |
| Threading | One shared `InferenceSession`, no shared mutable buffers, bounded concurrency option |
| Cutover | Done: the new embedder matched the Hugging Face reference on 34 of 34 items (the fork on 24 of 34), then the fork, the old provider project and both submodules were deleted |
| Model files | Not bundled. Downloaded on first use from a pinned Hugging Face revision (SHA-256 verified) into the Hugging Face hub cache, shared with other apps and mappable as a container volume |
