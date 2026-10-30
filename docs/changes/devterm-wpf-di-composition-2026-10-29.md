# DevTerm WPF DI composition (2026-10-29)

`OoBDev.DevTerm.Wpf` now composes through DI: `AddDevTermWpfShell()` registers `AddDevTermCore()`, `AddDevTermDevicePanels()`, the WPF colour picker, the demo `IPanelFactory` and `ShellViewModel`. `App.OnStartup` builds the provider (adding the `--scpi` factory when given) and resolves the shell. The project references `OoBDev.DevTerm.Core` and `OoBDev.DevTerm.Devices`.

Tests: `ServiceCollectionExtensionsTests` (2 Unit; the WPF test project has 10 passing). The device panels are registered but not yet shown: opening them needs a session, which comes with the WPF connection and main-window screens (still in the parity backlog).
