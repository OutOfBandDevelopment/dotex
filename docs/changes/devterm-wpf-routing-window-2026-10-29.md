# DevTerm WPF routing window (2026-10-29)

WPF slice 4, first part: `SessionViewModel` now carries the Core `SessionTab` when the session was built from options (profile or connection editor), and `ShellViewModel.ShowRoutingWindow` opens `RoutingWindow` through the `IRoutingDialog` seam (`WpfRoutingDialog` in the app, a fake in tests). `RoutingDialogViewModel` wraps `RoutingViewModel` with the `Apply`, `Start`, `Stop` and `SaveToProfile` commands (real `CommandBase` types), a `Message` line and the live `StatusText`. The window edits protocol, broker host, port and TLS.

Tests: `RoutingDialogTests` (2 Unit: the dialog opens for a profile session and Apply reports the validation problem; a loopback session without a tab cannot open it); WPF test project 22 passed. The window was built but not viewed running. Left in slice 4: rule list editing, routing history and counters, and the confirm prompt; also the stream monitor, playback and merged log windows.
