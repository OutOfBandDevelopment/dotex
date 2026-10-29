using System;

namespace OoBDev.DevTerm;

/// <summary>Raised when a transport moves between <see cref="ConnectionState"/> values.</summary>
public sealed class ConnectionStateChangedEventArgs(ConnectionState previous, ConnectionState current) : EventArgs
{
    /// <summary>Gets the state before the change.</summary>
    public ConnectionState Previous { get; } = previous;

    /// <summary>Gets the state after the change.</summary>
    public ConnectionState Current { get; } = current;
}
