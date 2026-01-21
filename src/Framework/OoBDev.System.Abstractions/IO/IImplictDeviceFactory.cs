using System.Collections.Generic;

namespace OoBDev.System.IO;

/// <summary>
/// Provides a factory for creating device adapters implicitly from definitions without requiring explicit device paths.
/// </summary>
public interface IImplictDeviceFactory : IDeviceFactory
{
    /// <summary>
    /// Gets a device adapter implicitly from the specified definition.
    /// </summary>
    /// <param name="definition">The device definition configuration.</param>
    /// <returns>A device adapter instance, or <c>null</c> if the device cannot be created.</returns>
    IDeviceAdapter? GetDevice(object? definition);

    /// <summary>
    /// Gets all device adapters that match the specified definition.
    /// </summary>
    /// <param name="definition">The device definition configuration.</param>
    /// <returns>A collection of device adapters matching the definition.</returns>
    IEnumerable<IDeviceAdapter> GetDevices(object? definition);
}
