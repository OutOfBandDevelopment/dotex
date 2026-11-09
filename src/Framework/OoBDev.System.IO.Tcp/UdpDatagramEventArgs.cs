using System;
using System.Net;

namespace OoBDev.System.IO.Tcp;

/// <summary>One received UDP datagram.</summary>
/// <param name="sender">Who sent it.</param>
/// <param name="payload">The datagram bytes.</param>
public sealed class UdpDatagramEventArgs(IPEndPoint sender, byte[] payload) : EventArgs
{
    /// <summary>Who sent the datagram.</summary>
    public IPEndPoint Sender { get; } = sender;

    /// <summary>The datagram bytes.</summary>
    public byte[] Payload { get; } = payload;
}
