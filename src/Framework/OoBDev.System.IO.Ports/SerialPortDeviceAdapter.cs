using System.IO;
using SerialPort = System.IO.Ports.SerialPort;

namespace OoBDev.System.IO.Ports;

/// <summary>
/// Adapter that wraps a <see cref="SerialPort"/> to provide a device interface.
/// </summary>
/// <param name="device">The serial port device to wrap.</param>
/// <remarks>
/// TODO: This should be disposable so it can be cleaned up correctly.
/// </remarks>
public class SerialPortDeviceAdapter(SerialPort device) : IBufferedDeviceAdapter
{
    /// <inheritdoc/>
    public string Type => nameof(SerialPort);

    /// <inheritdoc/>
    public string Path => device.PortName;

    /// <inheritdoc/>
    public int BytesToRead => device.BytesToRead;

    /// <inheritdoc/>
    public Stream Stream => device.BaseStream;

    //public bool IsOpen => _device.IsOpen;
    //public void Open() => _device.Open();

    /// <inheritdoc/>
    public bool TryOpen(out Stream? stream)
    {
        if (device.IsOpen)
        {
            stream = device.BaseStream;
            return true;
        }

        try
        {
            device.Open();
            stream = device.BaseStream;
            return true;
        }
        catch (IOException)
        {
            stream = null;
            return false;
        }
    }
}
