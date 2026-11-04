# OoBDev.DevTerm.Tui

Terminal.Gui front end for DevTerm, ported from dev-term `DevTerm.Console` onto `OoBDev.DevTerm.Core`.

## Description

A console executable that opens one device session (`AddDevTermFrontEnd` selects the transport from the command line) and offers it in several modes: the terminal window (`TuiMode`), a command-line mode (`CliMode`), the connection profile editor (`ConfigureMode`), manifest control panels (`ControlPanelMode`), the stream monitor, log playback, the converter tools and the expression picker. Themes come from `TuiTheme`; generated device forms come from `FormRenderer`.

## Verified

`OoBDev.DevTerm.Tui.Tests`: 355 Unit tests pass, 1 is skipped. Tests that need hardware, a broker or a real console are `DevLocal` and were not run. The screenshot tests write PNG and text captures under `docs/user-guide/images` and `artifacts/ui-review`.

## Not done

See the `DevTerm 100% parity` item in `BACKLOG.md`: scpi, k8055 and busylight panels (and the tests that need them), `BundleDevicePlugins`, the Windows BLE backend copy target, an interactive run of the Tui.
