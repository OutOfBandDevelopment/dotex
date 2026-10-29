using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.Configuration.Discovery;

/// <summary>One physical device: every service discovery found at one address, shown as a row with a badge per service.</summary>
/// <param name="Address">Dotted IPv4 address shared by the services.</param>
/// <param name="Name">The best human-readable label among the services.</param>
/// <param name="Services">One hit per service (transport and port), ordered by port.</param>
public sealed record NetworkDevice(string Address, string Name, IReadOnlyList<NetworkDeviceHit> Services)
{
    /// <summary>The advertised host name, when any service learned one.</summary>
    public string? Hostname => Services.Select(s => s.Hostname).FirstOrDefault(h => !string.IsNullOrEmpty(h));

    /// <summary>A badge per service, e.g. <c>vxi11, tcp:5025, mqtt:1883</c>.</summary>
    public string Badges => string.Join(", ", Services.Select(s => s.Port > 0 ? $"{s.Transport}:{s.Port}" : s.Transport));
}
