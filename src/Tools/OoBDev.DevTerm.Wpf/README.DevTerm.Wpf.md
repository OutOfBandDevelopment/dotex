# OoBDev.DevTerm.Wpf

WPF shell for device panels, rebuilt on the OoBDev libraries (phase 10 of [the device decoding plan](../../../docs/migration/device-decoding-migration-plan.md)).

## Description

A window with a panel list and the open panel's `FormView` from `OoBDev.UiDefinitions.Wpf`. A device adds an `IPanelFactory` in `App.OnStartup` to appear in the list; the shell opens it through `OpenPanelCommand`, so there is no code-behind logic.

| Type | Role |
|------|------|
| `ShellViewModel` | The registered factories, the open panel, and the `Open` command |
| `PanelViewModel` | A title and a `FormViewModel`; disposed when replaced |
| `IPanelFactory` | How a device contributes a panel |
| `ScpiPanelFactory` | A panel generated from a SCPI instrument profile (`OoBDev.Scpi.Panels`); the caller supplies the send function |
| `ScpiTcpExecutor` | Sends a SCPI request to an instrument's raw socket (default port 5025) and reads one reply line for a query |
| `DemoPanelFactory` | A panel generated from `DemoSettings`; stands in until device panels exist |

Start with `--scpi host[:port] [profile name]` to add a panel for an instrument on its raw socket; the profile is looked up in the SCPI catalog by name and defaults to the generic one. Other transports (serial, USBTMC, VXI-11) are not wired in yet.

## Verified

8 Unit tests (shell view model, SCPI factory, SCPI socket executor against a loopback listener). The window and a real instrument has not been started or looked at on screen.

## Notes

- `App` and `MainWindow` live in the namespace `DevTermApp`: the XAML compiler writes unqualified `System.*` names, which would resolve to the `OoBDev.System` namespace once a SCPI/IO assembly is referenced.
