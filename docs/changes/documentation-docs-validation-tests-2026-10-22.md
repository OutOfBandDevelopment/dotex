# Docs validation as MSTest tests

**Date:** 2026-10-22 · **Epic:** Documentation · **Status:** Complete

## Summary

`scripts/docs/validate-docs.py` is replaced by the test project `src/Tools/OoBDev.Docs.Tests`, so validation needs no Python and runs with the rest of the suite.

**Table 1 — Delivered**

| Check | Test | Category |
|-------|------|----------|
| Configured paths contain markdown | `Docs_ConfiguredPaths_ContainMarkdown` | Unit |
| PlantUML blocks start and end with matching `@start`/`@end`, no remote or C4 `!include`, no unbalanced `[` | `PlantUml_Blocks_AreWellFormed` | Unit |
| Relative links resolve | `Links_Relative_Resolve` | Unit |
| Figure and table captions are in order | `Captions_FiguresAndTables_AreSequential` | Unit |
| Every diagram renders through the PlantUML server | `PlantUml_Blocks_Render` | Integration |

## Configuration

- `DOCS_VALIDATE_PATHS`: `;`-separated paths relative to the repository root (default `docs/patterns-discovery`).
- `PLANTUML_URL`: renderer (default `http://localhost:18080`, the `dotex-plantuml` service in `containers/testing`).

## Verification

Unit tests (4) and the Integration test (1) passed against `docs/patterns-discovery` with the `plantuml/plantuml-server:jetty` container started locally. The whole `docs/` tree was not run; it has known old problems.
