using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.System.Net.Discovery;

namespace OoBDev.System.IO.Tcp;

/// <summary>Finds LXI instruments (<see cref="LxiDiscovery"/>) as an <see cref="INetworkDeviceProbe"/>, so the detect list takes LXI from one registration.</summary>
/// <param name="scan">The scan; defaults to <see cref="LxiDiscovery.ScanAsync"/>. A test passes its own.</param>
public sealed class LxiProbe(Func<TimeSpan, TimeSpan, CancellationToken, Task<IReadOnlyList<LxiDevice>>>? scan = null) : INetworkDeviceProbe
{
    private readonly Func<TimeSpan, TimeSpan, CancellationToken, Task<IReadOnlyList<LxiDevice>>> _scan =
        scan ?? ((listen, probe, ct) => LxiDiscovery.ScanAsync(listen, probe, cancellationToken: ct));

    /// <inheritdoc />
    public string Name => "lxi";

    /// <inheritdoc />
    public async Task<IReadOnlyList<NetworkDeviceHit>> ProbeAsync(TimeSpan listenFor, CancellationToken cancellationToken = default)
    {
        var probeTimeout = TimeSpan.FromMilliseconds(global::System.Math.Clamp(listenFor.TotalMilliseconds / 2, 300, 1500));
        var devices = await _scan(listenFor, probeTimeout, cancellationToken).ConfigureAwait(false);
        return [.. devices.Select(d => d.ScpiPort > 0
            ? new NetworkDeviceHit(d.Host, d.ScpiPort, "tcp", "lxi", d.Identity.Length > 0 ? d.Identity : d.Host, Name)
            : new NetworkDeviceHit(d.Host, d.Vxi11Port, "vxi11", "lxi", d.Host, Name))];
    }
}
