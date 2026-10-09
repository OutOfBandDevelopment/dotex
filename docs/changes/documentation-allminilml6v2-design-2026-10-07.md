# Documentation - AllMiniLmL6V2 Replacement Design

**Date:** 2026-10-07
**Epic:** AI (patterns discovery follow-up)
**Status:** ✅ COMPLETE (design only; no code written)
**Impact:** 5 documents under `docs/design/AllMiniLmL6V2/`

## Summary

Designed the replacement of the unmaintained `AllMiniLML6v2Sharp` fork with a first-party implementation that keeps the same `all-MiniLM-L6-v2` ONNX model, uses `Microsoft.ML.Tokenizers` and ONNX Runtime, exposes `IEmbeddingGenerator`, and is thread-safe by construction. Cutover is gated on compatibility and concurrency suites.

## Detailed Changes

- `docs/design/AllMiniLmL6V2/` with README, requirements (REQ-001…016), architecture (projects `OoBDev.Onnx.SentenceEmbeddings` and `OoBDev.SBert.AllMiniLmL6V2`, phases), api-design, testing-strategy.
- `TODO.md` backlog item links to the design; `CLAUDE.md` and `docs/changes/README.md` updated.

## Verification

- `python scripts/docs/validate-docs.py docs/design`: 5 files, 0 problems. Re-run on 2026-10-07 with the PlantUML server up: 0 problems, diagrams render.

## Related Documentation

- [Design](../design/AllMiniLmL6V2/README.md)
- [Extensions.AI spike](../patterns-discovery/08-spikes/01-extensions-ai.md)
