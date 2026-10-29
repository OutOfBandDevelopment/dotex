# LlmCodeGen response metadata

**Date:** 2026-10-29

`FolderPromptRunner` now writes `<template>.response.json` beside `<template>.response.md`: provider, model, template, folder, input file count, prompt and response character counts, start time (UTC), duration in milliseconds and the extracted file names. The runner takes an optional `TimeProvider` for tests.

**Not included:** token counts and finish reasons, because `IMessageCompletion.GetCompletionAsync(string, string)` returns text only.

**Verified:** `OoBDev.LlmCodeGen.Cli.Tests` Unit and Simulate, 28 passed (new `RunAsync_WritesResponseMetadataBesideTheResponse`).

Design: [api-design](../design/LlmCodeGen/api-design.md#output-layout). Readme: `README.LlmCodeGen.Cli.md`.
