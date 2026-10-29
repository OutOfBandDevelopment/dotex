using HidSharp;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.System.IO.Pipelines;

namespace OoBDev.System.IO.UsbHids;

/// <summary>
/// Adapter that wraps a USB HID device to provide a device interface.
/// </summary>
/// <param name="device">The USB HID device to wrap.</param>
public class UsbHidDeviceAdapter(HidDevice device) : StreamDeviceAdapter
{
    /// <inheritdoc/>
    public override string Type => nameof(HidDevice);

    /// <inheritdoc/>
    public override string Path => device.DevicePath;

    /// <inheritdoc/>
    protected override Task<Stream?> ConnectAsync(CancellationToken cancellationToken) =>
        Task.FromResult<Stream?>(device.TryOpen(out var stream) ? stream : null);
}
