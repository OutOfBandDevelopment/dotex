using System;
namespace OoBDev.System.IO.Bluetooth;

/// <summary>
/// The default registration when no per-OS BLE backend is available - resolvable, but fails clearly
/// at connect time rather than at DI registration time, and only once a user actually picks the
/// "ble" transport.
/// </summary>
public sealed class UnsupportedPlatformBleConnectionFactory : IBleConnectionFactory
{
    public IBleConnection Create(BleDeviceAdapterOptions options) =>
        throw new PlatformNotSupportedException(
            "No BLE adapter is available for this platform/build. On Windows this is picked up " +
            "automatically from OoBDev.System.IO.Bluetooth.Windows.dll when it's present next to the " +
            "executable; other platforms have no backend yet.");
}
