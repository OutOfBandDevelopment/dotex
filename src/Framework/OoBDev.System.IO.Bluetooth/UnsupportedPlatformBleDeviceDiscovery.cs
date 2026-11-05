using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace OoBDev.System.IO.Bluetooth;

/// <summary>
/// The default registration when no per-OS BLE backend is available. Returns an empty list rather
/// than throwing - unlike <see cref="UnsupportedPlatformBleConnectionFactory"/>, a discovery listing is
/// informational (a picker, or "--listbledevices"), and an empty result is a legitimate, renderable
/// answer to "what's out there" the same way it would be on a real backend that just finds nothing.
/// </summary>
public sealed class UnsupportedPlatformBleDeviceDiscovery : IBleDeviceDiscovery
{
    public IReadOnlyList<BleDeviceDescriptor> GetDevices() => [];
}
