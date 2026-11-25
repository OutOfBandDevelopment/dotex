using System.Collections.Generic;
namespace OoBDev.System.IO.Bluetooth;

/// <summary>
/// Enumerates a specific, already-discovered peripheral's GATT services/characteristics ("sub-device"
/// UUIDs) - a device picked via <see cref="IBleDeviceDiscovery"/> doesn't say which service/
/// characteristic UUIDs it actually exposes, and <see cref="BleDeviceAdapterOptions"/> defaults to Nordic
/// UART Service's, which not every "BLE serial" peripheral actually uses. Separate from
/// <see cref="IBleDeviceDiscovery"/> since this needs a specific, already-known <c>deviceId</c>
/// (a GATT session against one peripheral) rather than scanning for nearby ones.
/// </summary>
public interface IBleGattProfileExplorer
{
    IReadOnlyList<BleGattServiceDescriptor> GetServices(string deviceId);
}
