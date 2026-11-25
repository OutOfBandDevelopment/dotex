# OoBDev.DevTerm.Wpf

WPF front end for DevTerm on `OoBDev.DevTerm.Core`: session tabs with connect, send and output, device panels from `OoBDev.UiDefinitions.Wpf`, and the DevTerm tool windows. Every action is an `ICommand` class with injected dependencies and every window talks to its view model through a dialog interface, so the logic is tested without showing a window (rule `owner-decisions`, MVVM with command binding). Slice status is in [the parity status](../../../docs/design/DevTermParity/status.md).

## Contents

- [Shell](#shell)
- [Windows and dialogs](#windows-and-dialogs)
- [SCPI instruments](#scpi-instruments)
- [Tests](#tests)
- [Notes](#notes)

## Shell

`App` composes through `services.AddDevTermWpfShell()`, which registers the core, the device panels (`AddDevTermDevicePanels()`), plugins, the text presenters, the loopback transport, the profile store, the dialogs listed below and the shell view model.

**Table 1 — Shell types**

| Type | Role |
|------|------|
| `ShellViewModel` | The registered `IPanelFactory`s, the open panel and sessions, and the `Open` command |
| `SessionViewModel`, `SessionViewModelFactory` | One session tab: connect, disconnect, send, output, logging |
| `PanelViewModel`, `IPanelFactory` | A title and a `FormViewModel`, disposed when replaced; how a device or plugin contributes a panel |
| `DemoPanelFactory`, `DemoSettings` | A panel generated from settings, for flows that need no hardware |
| `WpfTheme`, `DarkControls.xaml`, `ThemeViewModel` | Light and dark themes with replacement control templates (stock chrome is hard-coded light) |

## Windows and dialogs

Each dialog is an interface (`IRoutingDialog`) with a `Wpf...Dialog` implementation that shows a `...Window` over a `...DialogViewModel`.

**Table 2 — Tool windows**

| Window | Does |
|--------|------|
| Connection editor | Edit connection profiles and scan for serial, USB, HID, LXI and network devices (`ConnectionEditorWindow`) |
| Configure device | Read and write a device's configuration with preview (`ConfigureDeviceWindow`) |
| Playback and merged log | Replay a session log; merge several logs (`PlaybackWindow`, `MergedLogWindow`) |
| Stream monitor | Capture and preview images and plot data seen in the byte stream, export, convert (`StreamMonitorWindow`) |
| Routing | Edit and run rules between a session and a broker (`RoutingWindow`) |
| Converter tools | Edit the external converters (`ConverterToolsWindow`) |
| Manifest picker and editor | Open or edit a device manifest (`ManifestPickerWindow`, `ManifestEditorWindow`) |
| Expression picker | Pick a value path for a manifest expression (`ExpressionPickerWindow`) |
| Theme builder | Build a theme from seed colours (`ThemeBuilderWindow`, `ThemeSeedPickerWindow`) |
| Instrument picker | Choose an SCPI instrument (`InstrumentPickerWindow`) |

Plugin installation and approval review, profile zip import and export, listening for UDP or peer sessions and session logging are commands on the main window.

## SCPI instruments

Start with `--scpi host[:port] [profile name]` to add a panel for an instrument on its raw socket. The profile is looked up in the SCPI catalog by name and defaults to the generic one. `ScpiPanelFactory` builds the panel from the profile (`OoBDev.Scpi.Panels`) and `ScpiTcpExecutor` sends requests to the socket (default port 5025), reading one reply line for a query. Other SCPI transports (serial, USBTMC, VXI-11) are reached through a connection profile and the instrument picker, not this switch.

## Tests

`OoBDev.DevTerm.Wpf.Tests` (about 180 test methods) is `[assembly: DoNotParallelize]` (WPF's XAML parser races across STA threads) and includes screenshot tests. Real instruments and a manual run on screen are covered by the hardware passes in `docs/applications/devterm/test/`, not by these tests.

## Notes

- `App` and `MainWindow` live in the namespace `DevTermApp`: the XAML compiler writes unqualified `System.*` names, which would resolve to the `OoBDev.System` namespace once a SCPI/IO assembly is referenced.
