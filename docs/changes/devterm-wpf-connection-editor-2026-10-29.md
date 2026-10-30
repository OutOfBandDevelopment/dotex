# DevTerm WPF connection editor window (2026-10-29)

WPF slice 1, last part: `ConnectionEditorWindow` hosts the Core `ConnectionEditorViewModel` (transport, serial port, host, TCP port, saved profile list with load, delete and save, connect, cancel). The shell reaches it through the `IConnectionEditorDialog` seam (`WpfConnectionEditorDialog` in the app, a fake in tests), so `ShellViewModel.NewConnection` stays retestable: it creates the editor through `ISessionViewModelFactory.CreateEditor()`, and a valid `Result` opens a session through `CreateFromOptions`. `MainWindow.xaml` has a "New connection..." button.

Tests: two Unit tests in `ShellSessionTests` (connect opens a session titled `loopback 1`; cancel opens nothing); WPF test project 20 passed. The two windows were built but not viewed running. Left: per-transport fields and device pickers (HID, USBTMC, BLE, LXI, presenters, serial settings) in the editor window, and refreshing the shell's profile buttons after a save.
