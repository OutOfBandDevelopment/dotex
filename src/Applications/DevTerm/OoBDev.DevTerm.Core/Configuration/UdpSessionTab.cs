using System;
using Microsoft.Extensions.Options;
using OoBDev.System.IO.Tcp;

namespace OoBDev.DevTerm.Configuration;

/// <summary>A tab around one bound UDP port, with its adapter so a front end can show each datagram's sender.</summary>
/// <param name="Tab">The tab; open its session to bind the port.</param>
/// <param name="Adapter">The UDP adapter; <see cref="UdpEndpointDeviceAdapter.DatagramReceived"/> reports each sender.</param>
public sealed record UdpSessionTab(SessionTab Tab, UdpEndpointDeviceAdapter Adapter)
{
    /// <summary>Composes a UDP session tab (see docs/design/Transports/transports.md, UDP).</summary>
    /// <param name="cliOptions">The options the session's presenters and parser come from.</param>
    /// <param name="host">The local address to bind; any address when empty.</param>
    /// <param name="port">The local port; 0 picks a free one.</param>
    /// <param name="target">Where sent bytes go (<c>host:port</c>); empty replies to the last sender.</param>
    public static UdpSessionTab Build(CliOptions cliOptions, string? host, int port, string? target = null)
    {
        ArgumentNullException.ThrowIfNull(cliOptions);
        var adapter = new UdpEndpointDeviceAdapter(Options.Create(new UdpEndpointOptions { Host = host, Port = port, Target = target }));
        return new UdpSessionTab(SessionTab.BuildForAdapter(cliOptions, adapter), adapter);
    }

    /// <summary>The line a front end shows for a received datagram.</summary>
    public static string Describe(UdpDatagramEventArgs datagram)
    {
        ArgumentNullException.ThrowIfNull(datagram);
        return $"Datagram from {datagram.Sender} ({datagram.Payload.Length} bytes)";
    }
}
