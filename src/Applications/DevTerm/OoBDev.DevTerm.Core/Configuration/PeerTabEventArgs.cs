using System;

namespace OoBDev.DevTerm.Configuration;

/// <summary>An accepted listener peer, wrapped as a tab.</summary>
/// <param name="remoteEndPoint">The peer's address as <c>host:port</c>.</param>
/// <param name="tab">The tab around the peer's open connection.</param>
public sealed class PeerTabEventArgs(string remoteEndPoint, SessionTab tab) : EventArgs
{
    /// <summary>The peer's address as <c>host:port</c>.</summary>
    public string RemoteEndPoint { get; } = remoteEndPoint;

    /// <summary>The tab around the peer's open connection.</summary>
    public SessionTab Tab { get; } = tab;
}
