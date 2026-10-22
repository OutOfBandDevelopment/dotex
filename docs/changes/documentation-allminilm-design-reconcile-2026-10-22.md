# AllMiniLmL6V2 design reconciled with the code

**Date:** 2026-10-22 · **Compared:** `docs/design/AllMiniLmL6V2/*` against `OoBDev.Onnx.SentenceEmbeddings`, `OoBDev.SBert.AllMiniLmL6V2` and their tests.

## Design updated to match the code

- Status of all five documents: implemented (was Draft, "Proposed").
- API design rewritten from the code: the full option table (`ModelFiles`, `ModelFileName`, `VocabFileName`, special tokens, `LayerNormalize`), the generator constructor without `TimeProvider`, `AllMiniLmL6V2Model.ApplyDefaults` instead of an options subclass, `SBertGlobals.DefaultSection`, and the real registration (unkeyed generator, `Replace` of the default `IEmbeddingProvider`, keyed provider).

## Design wanted, code lacks

Marked Planned and added to `BACKLOG.md`: telemetry (REQ-011), keyed generator registration with the builder middleware, `AddValidatedOptions`. The `ALLMINILM` alias is dropped because the framework is unreleased.

## Verification

`OoBDev.Docs.Tests` run on `docs/design/AllMiniLmL6V2`.
