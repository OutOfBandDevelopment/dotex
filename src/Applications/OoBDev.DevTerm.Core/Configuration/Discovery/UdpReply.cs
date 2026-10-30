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

/// <summary>One datagram received while listening, with the address it came from.</summary>
public sealed record UdpReply(IPAddress From, byte[] Data);
