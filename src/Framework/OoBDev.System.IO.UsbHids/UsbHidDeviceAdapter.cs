using HidSharp;
using System;
using System.IO;

namespace OoBDev.System.IO.UsbHids;

/// <summary>
/// Adapter that wraps a USB HID device to provide a device interface.
/// </summary>
/// <param name="device">The USB HID device to wrap.</param>
public class UsbHidDeviceAdapter(HidDevice device) : IDeviceAdapter
{
    /// <inheritdoc/>
    public string Type => nameof(HidDevice);

    /// <inheritdoc/>
    public string Path => device.DevicePath;

    /// <inheritdoc/>
    public Stream Stream { get => field ?? throw new ApplicationException($"Stream for {device} is not open"); private set; }

    //public void Open() => _device.Open();

    /// <inheritdoc/>
    public bool TryOpen(out Stream? stream)
    {
        if (device.TryOpen(out var s))
        {
            Stream = stream = s;
            return true;
        }
        else
        {
            stream = null;
            return false;
        }
    }
}
