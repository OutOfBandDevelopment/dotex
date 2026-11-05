using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO;

/// <summary>
/// An asynchronous, full-duplex connection to a device (serial, TCP, HID, USBTMC, BLE and so on) exposed as pipes so bytes
/// move without copies. The adapter knows nothing about decoders or applications.
/// </summary>
public interface IDeviceAdapter : IAsyncDisposable
{
    /// <summary>Gets the type of the device adapter.</summary>
    string Type { get; }

    /// <summary>Gets the path (port name, host and port, device path) associated with the device adapter.</summary>
    string Path { get; }

    /// <summary>Gets the current lifecycle state.</summary>
    ConnectionState State { get; }

    /// <summary>Raised on every state change.</summary>
    event EventHandler<ConnectionStateChangedEventArgs>? StateChanged;

    /// <summary>Gets the incoming bytes; valid once <see cref="State"/> is <see cref="ConnectionState.Open"/>.</summary>
    PipeReader Input { get; }

    /// <summary>Gets the outgoing bytes; valid once <see cref="State"/> is <see cref="ConnectionState.Open"/>.</summary>
    PipeWriter Output { get; }

    /// <summary>Opens the connection; throws and moves to <see cref="ConnectionState.Faulted"/> when the device cannot be opened.</summary>
    /// <param name="cancellationToken">A token to cancel the open.</param>
    Task OpenAsync(CancellationToken cancellationToken = default);

    /// <summary>Closes the connection.</summary>
    /// <param name="cancellationToken">A token to cancel waiting for the close.</param>
    Task CloseAsync(CancellationToken cancellationToken = default);
}
