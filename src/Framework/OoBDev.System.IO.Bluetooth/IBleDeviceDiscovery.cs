using System.Collections.Generic;
namespace OoBDev.System.IO.Bluetooth;

/// <summary>Enumerates nearby/paired BLE peripherals.</summary>
public interface IBleDeviceDiscovery
{
    IReadOnlyList<BleDeviceDescriptor> GetDevices();
}
