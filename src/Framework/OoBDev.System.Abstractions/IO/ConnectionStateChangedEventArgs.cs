using System;

namespace OoBDev.System.IO;

/// <summary>Describes a change of <see cref="IDeviceAdapter.State"/>.</summary>
/// <param name="previous">The state before the change.</param>
/// <param name="current">The state after the change.</param>
public sealed class ConnectionStateChangedEventArgs(ConnectionState previous, ConnectionState current) : EventArgs
{
    /// <summary>Gets the state before the change.</summary>
    public ConnectionState Previous { get; } = previous;

    /// <summary>Gets the state after the change.</summary>
    public ConnectionState Current { get; } = current;
}
