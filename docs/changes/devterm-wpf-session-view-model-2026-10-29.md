# DevTerm WPF session view model (2026-10-29)

First part of WPF slice 1 (session workflow): `SessionViewModel` in `OoBDev.DevTerm.Wpf` wraps a `Session` and exposes the decoded `OutputLines`, `State`, `SendText`, `ErrorMessage` and the `Connect`, `Disconnect` and `Send` commands (real `CommandBase` types, not lambdas). UI-thread marshalling uses the `SynchronizationContext` captured at construction, and failures from connect or send land in `ErrorMessage`.

Tests: `SessionViewModelTests` (4 Unit over the loopback adapter; WPF test project 14 passed). Still open for slice 1: the XAML session view and tab host in `MainWindow`, the connection list and editor window, and building sessions from `ConnectionEditorViewModel` through `ISessionFactory`.
