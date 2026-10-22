# LlmCodeGen provider selection

**Date:** 2026-10-22 · **Design:** [LlmCodeGen](../design/LlmCodeGen/README.md)

- `LlmCodeGenService.ResolveCompletion` holds the provider lookup that was inline in `ExecuteAsync`, so it can be tested.
- Found by the new tests: keyed lookup was case-sensitive, so `OLLAMA` failed while `Groq` worked. Provider names are now lower-cased, so any letter case resolves.
- `LlmCodeGenServiceTests` (Unit) covers `ollama`, `groq` and `groq-cloud` in several cases, and the unknown-provider error. `OoBDev.LlmCodeGen.Cli.Tests` passed, 27 of 27.

Not done: the live Ollama run (Integration) and the `<template>.response.json` provider metadata stay in BACKLOG.
