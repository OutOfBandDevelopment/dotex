using System.Collections.Generic;

namespace OoBDev.System.IO;

/// <summary>
/// Provides a factory for creating device adapters.
/// </summary>
public interface IDeviceFactory
{
    /// <summary>
    /// Determines whether a device can be created from the specified definition.
    /// </summary>
    /// <param name="definition">The device definition to check.</param>
    /// <returns><c>true</c> if a device can be created from the definition; otherwise, <c>false</c>.</returns>
    bool CanGetDevice(object? definition);

    /// <summary>
    /// Gets the names of all available devices.
    /// </summary>
    /// <returns>A collection of device names.</returns>
    IEnumerable<string> GetDeviceNames();

    /// <summary>
    /// Gets a device adapter for the specified device path and definition.
    /// </summary>
    /// <param name="devicePath">The path to the device.</param>
    /// <param name="definition">The device definition configuration.</param>
    /// <returns>A device adapter instance, or <c>null</c> if the device cannot be created.</returns>
    IDeviceAdapter? GetDevice(string devicePath, object? definition);
}
