using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.Net.Discovery;

/// <summary>Sends one datagram and collects the replies for a while; a seam so probes can be tested without a network.</summary>
public interface IUdpExchange
{
    Task<IReadOnlyList<UdpReply>> ExchangeAsync(IPEndPoint target, byte[] request, TimeSpan listenFor, CancellationToken cancellationToken);
}
