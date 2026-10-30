# K8055 and Busylight panels with their protocols (2026-10-29)

Owner decisions: panels use the generic `UiDefinition` and live with their protocols; contracts live in `OoBDev.System.Abstractions`; DevTerm Core stays free of device libraries.

## What changed

- New contracts in `OoBDev.System.Abstractions` (namespace `OoBDev.UiDefinitions`): `IDevicePanel`, `IDeviceDecoder`, `DeviceSend` (a write delegate instead of a DevTerm `Session`).
- `OoBDev.Velleman.K8055.Panels` and `OoBDev.Kuando.Busylight.Panels`: panel, UI definition, control surface and decoder, with tests (K8055 40 and Busylight 33 Unit tests in the whole projects).
- `OoBDev.DevTerm.Core`: generic `DevicePanelContribution`, `DeviceDecoderPresenter` and `TryAddDevicePanel<TPanel>()` (namespace `OoBDev.DevTerm.Panels`), 5 Unit tests.
- New `OoBDev.DevTerm.Devices` composition project: `AddDevTermDevicePanels()`, called by the Tui and Web hosts. The Wpf app has no DI composition yet.
- The 4 ignored Web tests are enabled again (Web Unit 47 passed, Tui Unit 355 passed, 1 skipped).

## Not run

DevLocal and hardware tests (no device attached).
