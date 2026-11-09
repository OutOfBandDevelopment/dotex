using System.Net;

namespace OoBDev.DevTerm.Configuration.Discovery;

/// <summary>One datagram received while listening, with the address it came from.</summary>
public sealed record UdpReply(IPAddress From, byte[] Data);
