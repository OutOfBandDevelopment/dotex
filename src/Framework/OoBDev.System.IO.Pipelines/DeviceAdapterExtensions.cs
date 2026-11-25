using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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

    /// <summary>Writes the bytes to <see cref="IDeviceAdapter.Output"/> and flushes them.</summary>
    /// <param name="adapter">The opened adapter.</param>
    /// <param name="data">The bytes to send.</param>
    /// <param name="cancellationToken">A token to cancel the write.</param>
    public static async Task WriteAsync(this IDeviceAdapter adapter, ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(adapter);
        await adapter.Output.WriteAsync(data, cancellationToken).ConfigureAwait(false);
    }
}
