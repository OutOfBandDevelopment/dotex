using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HidSharp;

namespace OoBDev.System.IO.UsbHids;

/// <summary>HidSharp-backed discovery.</summary>
public sealed class SystemUsbHidDiscovery : IUsbHidDiscovery
{
    public IReadOnlyList<UsbHidDeviceDescriptor> GetDevices() =>
        [.. DeviceList.Local.GetHidDevices().Select(ToDescriptor)];

    private static UsbHidDeviceDescriptor ToDescriptor(HidDevice device) => new(
        device.VendorID,
        device.ProductID,
        TryGet(device.GetProductName),
        TryGet(device.GetSerialNumber),
        device.DevicePath);

    // Reading these strings from the device can fail (permissions, a device that doesn't
    // implement the optional string descriptors, ...) — discovery should still list the device
    // by VID/PID rather than fail the whole listing over a missing label.
    private static string? TryGet(Func<string> getter)
    {
        try
        {
            return getter();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return null;
        }
    }
}
