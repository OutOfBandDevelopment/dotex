using System;
using System.IO;

namespace OoBDev.System.IO.UsbHids;

/// <summary>
/// Thin abstraction over a HidSharp device/stream so <see cref="UsbHidEndpointDeviceAdapter"/> can be unit
/// tested with a fake/mock instead of a real HID device.
/// </summary>
public interface IUsbHidDevice : IDisposable
{
    bool IsOpen { get; }

    /// <summary>The readable stream <see cref="UsbHidEndpointDeviceAdapter"/> pumps into a pipe (see <see cref="StreamToPipePump"/>).</summary>
    Stream BaseStream { get; }

    void Open();

    void Close();

    void Write(byte[] buffer, int offset, int count);
}
