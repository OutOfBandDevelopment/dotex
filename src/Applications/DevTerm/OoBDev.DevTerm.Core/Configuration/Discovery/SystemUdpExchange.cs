using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.Configuration.Discovery;

/// <summary>Sends from every up, non-loopback IPv4 adapter (multicast and broadcast leave through one interface at a time).</summary>
public sealed class SystemUdpExchange : IUdpExchange
{
    public async Task<IReadOnlyList<UdpReply>> ExchangeAsync(IPEndPoint target, byte[] request, TimeSpan listenFor, CancellationToken cancellationToken)
    {
        var replies = new List<UdpReply>();
        foreach (var local in LocalIPv4Addresses())
        {
            try
            {
                using var udp = new UdpClient(new IPEndPoint(local, 0)) { EnableBroadcast = true };
                udp.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, local.GetAddressBytes());
                await udp.SendAsync(request, target, cancellationToken).ConfigureAwait(false);
                using var window = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                window.CancelAfter(listenFor);
                while (true)
                {
                    var reply = await udp.ReceiveAsync(window.Token).ConfigureAwait(false);
                    replies.Add(new UdpReply(reply.RemoteEndPoint.Address, reply.Buffer));
                }
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // The listen window closing is the normal end of a probe on this adapter.
            }
            catch (SocketException)
            {
                // An adapter that can't send (VPN, virtual) contributes nothing.
            }
        }

        return replies;
    }

    private static IEnumerable<IPAddress> LocalIPv4Addresses() =>
        NetworkInterface.GetAllNetworkInterfaces()
            .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .SelectMany(n => n.GetIPProperties().UnicastAddresses)
            .Select(a => a.Address)
            .Where(a => a.AddressFamily == AddressFamily.InterNetwork);
}
