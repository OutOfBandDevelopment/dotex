# Image embeddings design reconciled with the code

**Date:** 2026-10-22 · **Compared:** `docs/design/ImageEmbeddings/*` against `OoBDev.Onnx.ImageEmbeddings`, `.Skia`, the three `OoBDev.Vision.*` presets and `OoBDev.Onnx.ImageEmbeddings.Tests`.

## Design updated to match the code

- API design: option names `ModelPath` and `ModelFileName` (were `ModelFolder` and `ModelFile`), defaults, `IntraOpThreads`, `Validate()`, and that registration lives in the preset projects.
- Testing strategy: the test classes that exist.

## Design wanted, code lacks

Nothing found; contracts, classifier and zero-shot classifier match.

## Verification

`OoBDev.Docs.Tests` run on `docs/design/ImageEmbeddings`.
