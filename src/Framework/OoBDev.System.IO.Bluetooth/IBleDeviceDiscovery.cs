using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace OoBDev.System.IO.Bluetooth;

/// <summary>Enumerates nearby/paired BLE peripherals.</summary>
public interface IBleDeviceDiscovery
{
    IReadOnlyList<BleDeviceDescriptor> GetDevices();
}
