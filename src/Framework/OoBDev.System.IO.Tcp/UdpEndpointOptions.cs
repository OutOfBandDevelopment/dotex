namespace OoBDev.System.IO.Tcp;

/// <summary>Configuration for a UDP listener session. See docs/design/Transports/transports.md (UDP).</summary>
public sealed class UdpEndpointOptions
{
    /// <summary>The local address or host name to bind; any address when empty.</summary>
    public string? Host { get; set; }

    /// <summary>The local port to bind; 0 picks a free one.</summary>
    public int Port { get; set; }

    /// <summary>
    /// Where sent bytes go (<c>host:port</c>). Empty means the sender of the last received datagram.
    /// </summary>
    public string? Target { get; set; }
}
