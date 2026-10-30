# DevTerm WPF merged log window (2026-10-29)

WPF slice 4, third part: every session the shell opens (loopback, profile or connection editor) is tracked in the shared Core `MergedSessionLog` (registered singleton) through one `AddSession` path in `ShellViewModel`. `ShowMergedLogWindow` opens `MergedLogWindow` through the `IMergedLogDialog` seam (`WpfMergedLogDialog` in the app, a fake in tests). `MergedLogDialogViewModel` lists the entries, follows `EntryAdded` on the captured `SynchronizationContext` and has a `Clear` command.

Tests: `MergedLogDialogTests` (2 Unit: a sent line shows in the list and Clear empties it; no log or dialog means the command cannot execute); WPF test project 26 passed. The window was built but not viewed running. Closed sessions are not untracked from the log yet (the shell does not remove sessions), and `PlaybackWindow` is still left in slice 4.
