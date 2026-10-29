using System;
using System.IO;

namespace OoBDev.System.IO.Pipelines;

/// <summary>Bridges an <see cref="IDeviceAdapter"/> to stream-based code.</summary>
public static class DeviceAdapterExtensions
{
    /// <summary>Wraps the pipes of an open adapter as one duplex stream; disposing the stream disposes the adapter.</summary>
    /// <param name="adapter">The opened adapter.</param>
    /// <returns>The duplex stream.</returns>
    public static Stream AsStream(this IDeviceAdapter adapter)
    {
        ArgumentNullException.ThrowIfNull(adapter);
        return new DeviceAdapterStream(adapter);
    }
}
