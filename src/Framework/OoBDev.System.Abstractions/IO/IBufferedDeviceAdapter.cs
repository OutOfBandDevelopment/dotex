namespace OoBDev.System.IO;

/// <summary>
/// Represents an adapter for a device that supports buffered operations.
/// </summary>
public interface IBufferedDeviceAdapter : IDeviceAdapter
{
    /// <summary>
    /// Gets the number of bytes available to read from the buffer.
    /// </summary>
    int BytesToRead { get; }
}
