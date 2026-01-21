using HidSharp;
using System;
using System.IO;

namespace OoBDev.System.IO.UsbHids;

public class UsbHidDeviceAdapter(HidDevice device) : IDeviceAdapter
{
    public string Type => nameof(HidDevice);
    public string Path => device.DevicePath;

    public Stream Stream { get => field ?? throw new ApplicationException($"Stream for {device} is not open"); private set; }

    //public void Open() => _device.Open();
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
