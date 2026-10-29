# DevTerm control contracts and transport helpers

Date: 2026-10-29. Branch `dev/example-ui-tests`.

- `OoBDev.DevTerm.Core` gains the control contracts (`IControlSurface`, `IDevicePanelContribution`, `IInstrumentPanelProvider` and helpers, `IDeviceConfigEditor` with `DeviceConfigSession`, `ICommandPreview`, `ParameterValueList`) and the transport helpers `IComPortControl`, `WriteDelayStream` and `ConnectionStateChangedEventArgs`. Core now references `OoBDev.UiDefinitions`.
- 15 tests added (11 ported, 4 new for `WriteDelayStream`); `OoBDev.DevTerm.Core.Tests` 210 passing, 5 inconclusive (Java/Go not installed).
- Not done: session servers and clients (pipe, TCP share, HTTP control) and `SessionCommands`.
