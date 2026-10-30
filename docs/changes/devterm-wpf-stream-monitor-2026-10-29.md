# DevTerm WPF stream monitor window (2026-10-29)

WPF slice 4, second part: `ShellViewModel.ShowStreamMonitorWindow` tracks the selected session in the shared Core `StreamMonitor` (registered singleton, exports go to `DevTermUserDataPaths.ExportsDirectory`) and opens `StreamMonitorWindow` through the `IStreamMonitorDialog` seam (`WpfStreamMonitorDialog` in the app, a fake in tests). `StreamMonitorDialogViewModel` exposes `IsRunning`, the capture lines and the `Start` and `Stop` commands (real `CommandBase` types); capture and state events are marshalled with the captured `SynchronizationContext`.

Tests: `StreamMonitorDialogTests` (2 Unit: start and stop toggle `IsRunning` for a loopback session; no session means the command cannot execute); WPF test project 24 passed. No capture was exercised end to end and the window was built but not viewed running. Left: capture preview, export and convert actions, per-device panes, `PlaybackWindow` and `MergedLogWindow`.
