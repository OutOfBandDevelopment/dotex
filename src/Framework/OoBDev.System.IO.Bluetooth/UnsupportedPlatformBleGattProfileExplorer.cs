using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace OoBDev.System.IO.Bluetooth;

/// <summary>
/// The default registration when no per-OS BLE backend is available. Returns an empty list rather
/// than throwing, same reasoning as <see cref="UnsupportedPlatformBleDeviceDiscovery"/>.
/// </summary>
public sealed class UnsupportedPlatformBleGattProfileExplorer : IBleGattProfileExplorer
{
    public IReadOnlyList<BleGattServiceDescriptor> GetServices(string deviceId) => [];
}
