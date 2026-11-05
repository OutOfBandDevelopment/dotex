using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.System.IO.Pipelines;
using SerialPort = global::System.IO.Ports.SerialPort;

namespace OoBDev.System.IO.Ports;

/// <summary>
/// Adapter that wraps a <see cref="SerialPort"/> to provide a device interface.
/// </summary>
/// <param name="device">The serial port device to wrap.</param>
public class SerialPortDeviceAdapter(SerialPort device) : StreamDeviceAdapter, IBufferedDeviceAdapter
{
    /// <inheritdoc/>
    public override string Type => nameof(SerialPort);

    /// <inheritdoc/>
    public override string Path => device.PortName;

    /// <inheritdoc/>
    public int BytesToRead => device.IsOpen ? device.BytesToRead : 0;

    /// <inheritdoc/>
    protected override Task<Stream?> ConnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!device.IsOpen) device.Open();
            return Task.FromResult<Stream?>(device.BaseStream);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            // A port held by another process throws UnauthorizedAccessException on Windows; a removed adapter throws IOException.
            return Task.FromResult<Stream?>(null);
        }
    }

    /// <inheritdoc/>
    protected override void Disconnect()
    {
        if (device.IsOpen) device.Close();
    }
}
