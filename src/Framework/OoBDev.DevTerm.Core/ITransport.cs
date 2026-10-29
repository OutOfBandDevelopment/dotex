using OoBDev.System.IO;
using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.DevTerm;

/// <summary>
/// An asynchronous, full-duplex connection to a device (serial, TCP, HID, USBTMC, BLE and so on).
/// Sits beside <see cref="OoBDev.System.IO.IDeviceAdapter"/>: that contract is a synchronous stream used by the
/// device libraries, this one adds a lifecycle and a pipe for sessions and shells.
/// </summary>
public interface ITransport : IAsyncDisposable
{
    /// <summary>Gets the current lifecycle state.</summary>
    ConnectionState State { get; }

    /// <summary>Raised on every state change.</summary>
    event EventHandler<ConnectionStateChangedEventArgs>? StateChanged;

    /// <summary>Gets the incoming bytes; valid once <see cref="State"/> is <see cref="ConnectionState.Open"/>.</summary>
    PipeReader Input { get; }

    /// <summary>Opens the connection.</summary>
    Task OpenAsync(CancellationToken cancellationToken = default);

    /// <summary>Closes the connection.</summary>
    Task CloseAsync(CancellationToken cancellationToken = default);

    /// <summary>Sends bytes to the device.</summary>
    Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default);
}
