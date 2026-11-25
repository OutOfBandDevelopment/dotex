using System.Collections.Generic;

namespace OoBDev.System.IO.UsbHids;

/// <summary>Enumerates connected HID devices.</summary>
public interface IUsbHidDiscovery
{
    /// <summary>Lists the HID devices currently connected.</summary>
    /// <returns>One descriptor per device.</returns>
    IReadOnlyList<UsbHidDeviceDescriptor> GetDevices();
}
