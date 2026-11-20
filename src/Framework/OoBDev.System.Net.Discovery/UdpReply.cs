using System.Net;

namespace OoBDev.System.Net.Discovery;

/// <summary>One datagram received while listening, with the address it came from.</summary>
public sealed record UdpReply(IPAddress From, byte[] Data);
