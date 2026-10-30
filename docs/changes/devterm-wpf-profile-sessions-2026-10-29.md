# DevTerm WPF profile sessions (2026-10-29)

WPF slice 1, next part: the shell opens sessions from saved connection profiles. `ISessionViewModelFactory` gained `ListProfiles()` and `CreateFromProfile(name)`; `SessionViewModelFactory` takes the `ConnectionProfileStore` (registered by `AddDevTermWpfShell()` with the default profiles directory) and builds the session through `DevTermSessionBuilder.Build`. `ShellViewModel` exposes `ProfileNames` and the `OpenProfileSession` command (`OpenProfileSessionCommand`, parameter is the profile name). `MainWindow.xaml` lists one button per profile in the left column.

Tests: two Unit tests in `ShellSessionTests` (a saved loopback profile opens a session titled by the profile; no factory means the command cannot execute); WPF test project 18 passed. The XAML was built but not viewed in a running window. Left in slice 1: the connection editor window over `ConnectionEditorViewModel`; the profile list is read once at startup, not refreshed.
