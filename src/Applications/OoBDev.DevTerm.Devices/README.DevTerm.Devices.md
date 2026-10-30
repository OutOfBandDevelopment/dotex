# OoBDev.DevTerm.Devices

Composition bundle that registers the device panels DevTerm ships with (Velleman K8055, Kuando Busylight).
Core stays free of device libraries; each host calls `services.AddDevTermDevicePanels()` next to `AddDevTermFrontEnd`.
Add a device by referencing its protocol library and calling `TryAddDevicePanel<TPanel>()` here.
SCPI: `ScpiReplyPresenter`, auto-detect, control surface and `ScpiInstrumentPanelProvider` (namespace `OoBDev.DevTerm.Devices.Scpi`) are registered by the same call, over `OoBDev.Scpi`.
