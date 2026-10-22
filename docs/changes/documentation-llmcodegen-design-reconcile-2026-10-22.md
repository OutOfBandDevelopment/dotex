# LlmCodeGen design reconciled with the code

**Date:** 2026-10-22 · **Compared:** `docs/design/LlmCodeGen/*` against `src/Tools/OoBDev.LlmCodeGen.Cli` and its tests.

## Design updated to match the code

- Status: implemented.
- API design: nullable `[Required]` options, `ValidateOnStart`, the in-memory Ollama default, the bundled templates, and the real output file names (`<template>.prompt.md`, `<template>.response.md`).
- Testing strategy: the 13 existing tests named.

## Design wanted, code lacks

Marked Planned and added to `BACKLOG.md`: extractor edge-case tests, options validation, provider selection, a live Ollama run, and `<template>.response.json`.

## Verification

`OoBDev.Docs.Tests` run on `docs/design/LlmCodeGen`.
