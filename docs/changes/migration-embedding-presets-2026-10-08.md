# Embedding presets for MPNet and Nomic, shared Hugging Face cache, version policy

**Date:** 2026-10-08 · **Epic:** AI embeddings · **Status:** Complete (GitHub run not yet observed)

## Summary

Two more models run on the in-process ONNX runner, `all-mpnet-base-v2` (768 dimensions) and `nomic-embed-text-v1.5` (768, Matryoshka). Their files use the same Hugging Face hub cache layout as the Python libraries, so a machine downloads each model once for every app. Both were compared with the Hugging Face originals. The same session also changed the version policy and the CI build order.

## Contents

- [What was built](#what-was-built)
- [Verification](#verification)
- [Version policy](#version-policy)
- [Follow-up](#follow-up)

## What was built

**Table 1 — New projects and runner options**

| Item | Purpose |
|------|---------|
| `OoBDev.SBert.AllMpnetBaseV2` | Preset: `sentence-transformers/all-mpnet-base-v2` at a pinned revision, `onnx/model.onnx` (about 420 MB), 384 token limit |
| `OoBDev.SBert.NomicEmbedTextV1_5` | Preset: `nomic-ai/nomic-embed-text-v1.5` at a pinned revision, `onnx/model.onnx` (about 520 MB), default prefix `search_document: `, sizes 768 to 64 |
| `HuggingFaceHubCache` | Cache root (`HF_HUB_CACHE`, `HF_HOME`, `XDG_CACHE_HOME`, `~/.cache/huggingface/hub`), snapshot folder and pinned download sources; the MiniLM preset uses it too |
| `ClsToken`, `SepToken`, `UnkToken`, `PadToken`, `MaskToken` | Special tokens are options now (MPNet uses `<s>`, `</s>`, `<pad>`); the padding id fills unused positions because MPNet derives position ids from it |
| `LayerNormalize` | Layer normalisation of the pooled vector before truncation, as the Nomic model card requires |
| Downloader | Accepts file names with a sub folder (`onnx/model.onnx`) |

Registration is keyed per model (`all-mpnet-base-v2`, `nomic-embed-text-v1.5`), so several models can be registered side by side; the first one added is also the unkeyed default.

## Verification

- `scripts/embeddings/make-reference-vectors.py <model> <output.json>` writes reference vectors from the original models. MPNet uses `sentence-transformers` (PyTorch weights). Nomic runs the official ONNX file with the Hugging Face tokenizer and the model card steps (mean pooling, layer norm, truncate, normalise), because the sentence-transformers wrapper needs `trust_remote_code`.
- `PresetReferenceTests` (Integration category, downloads the models): MPNet 35 of 35 items, Nomic 34 of 34 at 768 dimensions and 34 of 34 at 256 dimensions, all at cosine 0.999 or better (minimum rounds to 1.00000).
- A cold download into an empty `HF_HUB_CACHE` produced the standard `models--org--name/snapshots/revision/onnx/model.onnx` layout and passed the SHA-256 check.
- 33 runner tests pass, including the earlier MiniLM comparison, so the tokenizer change did not move it.

## Version policy

`GitVersion.yml`: major is bumped by hand (`+semver: major` in a commit message), main bumps minor on every merge and is tagged `vX.Y.Z`, other branches build `major.minor.<commits past the last main tag>-<branch>` and are no longer tagged (branch tags would reset the commit count). The workflow computes the branch version from `commitsSinceVersionSource`.

## Follow-up

- Done 2026-10-09: the MiniLM preset now uses the official `sentence-transformers/all-MiniLM-L6-v2` repo (`onnx/model.onnx`, revision `1110a243fdf4706b3f48f1d95db1a4f5529b4d41`, same `vocab.txt`) and was re-compared with the Python vectors.
- Done 2026-10-09: the MiniLM tests that load the model moved from Unit to Integration.
- Done 2026-10-09: the 60 SentenceEmbeddings tests (accent cases included) pass in a Linux container (`mcr.microsoft.com/dotnet/sdk:10.0`, with `-p:DisableGitVersionTask=true` when the copy has no `.git`). The example web API run remains unverified.

[↑ Change index](README.md)
