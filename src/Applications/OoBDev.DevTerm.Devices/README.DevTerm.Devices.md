# OoBDev.DevTerm.Devices

Composition bundle that registers the device panels DevTerm ships with (Velleman K8055, Kuando Busylight).
Core stays free of device libraries; each host calls `services.AddDevTermDevicePanels()` next to `AddDevTermFrontEnd`.
Add a device by referencing its protocol library and calling `TryAddDevicePanel<TPanel>()` here.
