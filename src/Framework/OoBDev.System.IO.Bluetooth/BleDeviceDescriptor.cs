namespace OoBDev.System.IO.Bluetooth;

/// <summary>
/// One BLE peripheral as seen during discovery, e.g. for a "--listbledevices" CLI action or a GUI
/// device picker. <paramref name="DeviceId"/> is whatever opaque identifier the underlying OS
/// backend needs to reconnect to this same peripheral later (a WinRT device id on Windows, a BlueZ
/// D-Bus object path on Linux, ...) - it's saved verbatim into <see cref="BleDeviceAdapterOptions.DeviceId"/>,
/// never parsed or reconstructed by cross-platform code.
/// </summary>
public sealed record BleDeviceDescriptor(string DeviceId, string? Name);
