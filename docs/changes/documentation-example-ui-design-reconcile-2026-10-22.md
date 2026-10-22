# Example UI design reconciled with the code

**Date:** 2026-10-22 · **Compared:** `docs/design/example-ui/*` against `src/Examples/OoBDev.Example.Web` (`src/layout/nav.ts`, `src/pages`, `src/router`).

## Design updated to match the code

- Screen index: real routes and status for each screen (Dashboard, RAG on `api/Rag`, Cache, Vector stores, Image embeddings, Vectors and matrices, Expression calculator and Security helpers inside the Toolbox, Operations).
- Screens built beyond the design (Telemetry, Model compare, Text tools) added to the index.
- Shell design: the navigation as built (Table 2) beside the target.

## Design wanted, code lacks

Kept and marked Planned or open: Search (controller must be restored), Identity management, Queue adapters and blob storage, Data access, Semantic Kernel, Formats, CLI catalog. All are already tracked in `future-example.md`.

## Verification

`OoBDev.Docs.Tests` run on `docs/design/example-ui`. Pages were compared by reading `nav.ts` and the `src/pages` listing; the app was not run.
