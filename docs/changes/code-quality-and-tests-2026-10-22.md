# Analyzer cleanup, Azurite queue tests, search example and local CI runner

**Date:** 2026-10-22 · **Epic:** Code quality and test health · **Status:** Complete

## Summary

A clean rebuild has no analyzer warnings; the remaining 8 build warnings are listed in [BACKLOG.md](../../BACKLOG.md).

**Table 1 — Changes**

| Area | Change |
|------|--------|
| Example web API | Public models use `IList` and `IDictionary` (MA0016); controllers use `TimeProvider`, ordinal comparers and invariant parsing; null navigator guarded |
| Analyzer settings | `src/.editorconfig` exempts `Tools` and `Examples` from MA0048 as well as MA0004; `SqlVectorStoreProvider` suppresses MA0004 for `await using` |
| Vulnerable transitive package | `OoBDev.Data.Vectors.SqlStore` references `System.Security.Cryptography.Pkcs` directly (pinned centrally) because transitive pinning is off |
| Azure Storage queue tests | The commented-out sender test is replaced by two Integration tests against Azurite (`SendMessage_Azurite_ReceivesSameText`, `ReceiveMessage_EmptyQueue_ReturnsNothing`) with unique queue names and cleanup |
| Search example | `SearchAnnotationsExampleTests` shows `[NotFilterable]`, `[NotSortable]`, `[NotSearchable]` and `[Searchable]` through `ExpressionTreeBuilder<T>` (3 Unit tests) |
| Local CI | `scripts/ci/new-runsettings.ps1` rewrites `.runsettings` ports for overridden `TEST_PORT_<NAME>` values; `run-tests.ps1` uses it (fixed a bug where several overrides collapsed into one) |
| Backlog hygiene | BotChat item closed (already removed), the work list split into [BACKLOG.md](../../BACKLOG.md) and [TODO.md](../../TODO.md), earlier owner questions merged into [OPEN-QUESTIONS.md](../../OPEN-QUESTIONS.md) |

## Not verified

`scripts/ci/run-tests.ps1` has not been run end to end, and the Service Bus emulator tests were not run.
