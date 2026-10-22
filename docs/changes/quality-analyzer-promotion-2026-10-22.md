# Analyzer rules promoted to warning

**Date:** 2026-10-22

- Promoted to `warning` in `src/.editorconfig`: MA0070, MA0084, MA0132, MA0144, MA0151, MA0158 and MA0169. The 19 sites they flagged were fixed (shadowing locals renamed, `System.Threading.Lock`, `OperatingSystem.IsWindows()`, `ReferenceEquals`, explicit `DateTimeOffset`). Also fixed MA0002 and MA0016 in the role-right types.
- Second pass: MA0091, MA0099, MA0134 and MA0147 also promoted after fixing their 7 sites. `StreamDevice` error events now pass the device as sender (was the adapter stream), `EFileAttributes` gained `None = 0`, `TaskEx.RunSync` uses a local async function instead of async-void delegates (a thrown exception still comes out of `RunSync` as an `AggregateException`), `AsyncManualResetEvent.Set` discards the start task explicitly, and `DirectoryRecord.IsDirectory` uses `HasFlag`.
- Verified: full solution build has no analyzer warnings; unit tests pass for AspNetCore (21), System (1762, 165 skipped) and MessageQueueing (15).
