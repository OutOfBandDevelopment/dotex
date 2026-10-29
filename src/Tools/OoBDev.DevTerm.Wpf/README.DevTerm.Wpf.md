# OoBDev.DevTerm.Wpf

WPF shell for device panels, rebuilt on the OoBDev libraries (phase 10 of [the device decoding plan](../../../docs/migration/device-decoding-migration-plan.md)).

## Description

A window with a panel list and the open panel's `FormView` from `OoBDev.UiDefinitions.Wpf`. A device adds an `IPanelFactory` in `App.OnStartup` to appear in the list; the shell opens it through `OpenPanelCommand`, so there is no code-behind logic.

| Type | Role |
|------|------|
| `ShellViewModel` | The registered factories, the open panel, and the `Open` command |
| `PanelViewModel` | A title and a `FormViewModel`; disposed when replaced |
| `IPanelFactory` | How a device contributes a panel |
| `DemoPanelFactory` | A panel generated from `DemoSettings`; stands in until device panels exist |

Transports (serial, USB, network) are phases 8 and 9, so no device is connected yet.

## Verified

4 Unit tests on the shell view model. The window itself has not been started or looked at on screen.
