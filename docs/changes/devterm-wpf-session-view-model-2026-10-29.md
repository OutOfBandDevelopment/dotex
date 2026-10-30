# DevTerm WPF session view model (2026-10-29)

First part of WPF slice 1 (session workflow): `SessionViewModel` in `OoBDev.DevTerm.Wpf` wraps a `Session` and exposes the decoded `OutputLines`, `State`, `SendText`, `ErrorMessage` and the `Connect`, `Disconnect` and `Send` commands (real `CommandBase` types, not lambdas). UI-thread marshalling uses the `SynchronizationContext` captured at construction, and failures from connect or send land in `ErrorMessage`.

Tests: `SessionViewModelTests` (4 Unit over the loopback adapter; WPF test project 14 passed). Still open for slice 1: the XAML session view and tab host in `MainWindow`, the connection list and editor window, and building sessions from `ConnectionEditorViewModel` through `ISessionFactory`.

## Shell tab host

`ShellViewModel` now holds `Sessions`, `SelectedSession` and a `NewLoopbackSession` command (a real `CommandBase` type) that uses `ISessionViewModelFactory` (`SessionViewModelFactory`: loopback adapter, `ISessionFactory`, the ASCII presenter). `AddDevTermWpfShell()` also registers the text presenters and the loopback adapter. `MainWindow.xaml` shows the session list, output, send box, connect and disconnect buttons and the error line beside the panel form. Tests: `ShellSessionTests` (2 Unit; WPF test project 16 passed). The XAML was built but not looked at in a running window.
