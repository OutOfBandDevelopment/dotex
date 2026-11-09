using System.Collections.Generic;

namespace OoBDev.System.IO.UsbTmc;

/// <summary>
/// Enumerates USBTMC-class devices currently attached to the system.
/// </summary>
public interface IUsbTmcDeviceDiscovery
{
    /// <summary>
    /// Returns every attached USB device that exposes a USBTMC interface
    /// (interface class 0xFE, sub-class 0x03) in its active configuration.
    /// </summary>
    IReadOnlyList<UsbTmcDeviceDescriptor> GetDevices();
}
