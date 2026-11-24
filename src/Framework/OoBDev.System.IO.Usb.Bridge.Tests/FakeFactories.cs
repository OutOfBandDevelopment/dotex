using System;
using System.IO;
using OoBDev.System.IO.UsbHids;
using OoBDev.System.IO.UsbTmc;

namespace OoBDev.System.IO.Usb.Bridge.Tests;

internal sealed class FakeTmcFactory(FakeUsbTmcDevice? device, Exception? failure = null) : IUsbTmcDeviceFactory
{
    public UsbTmcDeviceAdapterOptions? Options { get; private set; }

    public IUsbTmcDevice Create(UsbTmcDeviceAdapterOptions options)
    {
        Options = options;
        return failure is null ? device! : throw failure;
    }
}

internal sealed class FakeHidDevice(Stream input) : IUsbHidDevice
{
    public byte[]? Written { get; private set; }
    public bool IsOpen { get; private set; }
    public Stream BaseStream => input;
    public void Open() => IsOpen = true;
    public void Close() => IsOpen = false;
    public void Write(byte[] buffer, int offset, int count) => Written = buffer[offset..(offset + count)];
    public void Dispose() => IsOpen = false;
}

internal sealed class FakeHidFactory(FakeHidDevice device) : IUsbHidDeviceFactory
{
    public IUsbHidDevice Create(UsbHidEndpointOptions options) => device;
}
