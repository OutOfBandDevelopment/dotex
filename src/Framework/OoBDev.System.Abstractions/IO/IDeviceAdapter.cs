using System.IO;

namespace OoBDev.System.IO;

/// <summary>
/// Represents a generic device adapter interface.
/// </summary>
public interface IDeviceAdapter
{
    /// <summary>
    /// Gets the type of the device adapter.
    /// </summary>
    string Type { get; }

    /// <summary>
    /// Gets the path associated with the device adapter.
    /// </summary>
    string Path { get; }

    /// <summary>
    /// Attempts to open a stream associated with the device adapter.
    /// </summary>
    /// <param name="stream">When this method returns, contains the stream associated with the device adapter, if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the stream was successfully opened; otherwise, <see langword="false"/>.</returns>
    bool TryOpen(out Stream? stream);

    /// <summary>
    /// Gets the stream associated with the device adapter.
    /// </summary>
    Stream Stream { get; }
}
