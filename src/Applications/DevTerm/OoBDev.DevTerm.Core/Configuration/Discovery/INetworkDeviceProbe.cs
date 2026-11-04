using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.Configuration.Discovery;

/// <summary>A single way of finding network devices (see docs/design/NetworkDevices/network-device-discovery.md).</summary>
public interface INetworkDeviceProbe
{
    string Name { get; }

    /// <summary>Looks for devices for up to <paramref name="listenFor"/>. Never throws for an unreachable network; returns what it found.</summary>
    Task<IReadOnlyList<NetworkDeviceHit>> ProbeAsync(TimeSpan listenFor, CancellationToken cancellationToken = default);
}
