# Analyzer rules promoted to warning

**Date:** 2026-10-22

- Promoted to `warning` in `src/.editorconfig`: MA0070, MA0084, MA0132, MA0144, MA0151, MA0158 and MA0169. The 19 sites they flagged were fixed (shadowing locals renamed, `System.Threading.Lock`, `OperatingSystem.IsWindows()`, `ReferenceEquals`, explicit `DateTimeOffset`). Also fixed MA0002 and MA0016 in the role-right types.
- Left at `suggestion` because they still flag code: MA0091 (2, `StreamDevice` event sender), MA0099 (2, enum zero values), MA0134 (1, `AsyncManualResetEvent`), MA0147 (2, `TaskEx` async delegates).
- Verified: full solution build has no analyzer warnings; unit tests pass for AspNetCore (21), System (1762, 165 skipped) and MessageQueueing (15).
