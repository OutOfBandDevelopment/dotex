# In-process all-MiniLM-L6-v2 embedder replaces the fork

**Date:** 2026-10-08 · **Epic:** AI embeddings · **Status:** Complete (GitHub CI run not yet observed)

## Summary

The unmaintained `AllMiniLML6v2Sharp` fork and the SBert model submodule are replaced by a first-party, thread-safe embedder on ONNX Runtime. The model is no longer bundled: it is downloaded from a pinned Hugging Face revision on first use.

## Contents

- [What was built](#what-was-built)
- [Verification](#verification)
- [Removed](#removed)
- [Follow-up](#follow-up)

## What was built

- `OoBDev.Onnx.SentenceEmbeddings`: `IEmbeddingGenerator<string, Embedding<float>>` on ONNX Runtime, first-party BERT tokenizer (basic tokenizer and WordPiece), masked mean pooling, bounded concurrency, model download (`ModelDownloader`).
- `OoBDev.SBert.AllMiniLmL6V2`: preset with the pinned revision `75251058ddd779e3a744f87fdf63fb39681aec16`, SHA-256 hashes, keyed providers `all-minilm-l6-v2` (and legacy `ALLMINILM`), `TryAddAllMiniLmL6V2Services`.
- The model goes to the Hugging Face hub cache layout (`HF_HUB_CACHE`, `HF_HOME/hub`, `XDG_CACHE_HOME/huggingface/hub`, `~/.cache/huggingface/hub`), shared with Python tools and mappable as a container volume.
- `scripts/embeddings/make-reference-vectors.py` generates reference vectors from the original model.

## Verification

- 34 of 34 reference items match the Hugging Face model (cosine at least 0.999); the fork matched 24 of 34 (wrong on accents, Polish, Korean, Japanese, `C#`, dates, URLs, literal special tokens and long texts).
- Speed on 300 items: fork 18.75 s, new 2.6 s.
- 21 runner tests and 3 preset tests pass; the whole solution builds with 0 errors.

## Removed

- Submodules `src/ExternalServices/AllMiniLML6v2Sharp` and `.../OoBDev.SBert.AllMiniLML6v2Sharp/model`; `.gitmodules` is empty and deleted.
- Project `OoBDev.SBert.AllMiniLML6v2Sharp` and the fork projects from the solution; the `libtorch-cpu-win-x64` and `Microsoft.ML` package versions; the fork-only build rules in `Directory.Build.props` and `Directory.Build.targets`.
- Fork comparison tests; submodule init scripts and CI submodule steps.

## Follow-up

- Run the example web API end to end; test on Linux (accent stripping needs ICU).
- Presets for other models (MPNet, Nomic) can reuse the runner.

[↑ Change index](README.md)
