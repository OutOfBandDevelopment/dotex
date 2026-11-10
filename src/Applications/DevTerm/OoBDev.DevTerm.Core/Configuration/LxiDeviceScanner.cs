using System;
using System.Collections.Generic;
using System.Linq;
using OoBDev.DevTerm.Configuration.Discovery;
using OoBDev.System.IO.Tcp;

namespace OoBDev.DevTerm.Configuration;

/// <summary>
/// Runs a one-off LXI discovery scan for a front end's device picker (see <see cref="LxiDiscovery"/>). A scan listens for replies
/// for a couple of seconds, so callers on a UI thread should run it on a background thread, like <see cref="BleDeviceScanner"/>.
/// </summary>
public static class LxiDeviceScanner
{
    /// <summary>Scans for LXI instruments.</summary>
    public static IReadOnlyList<LxiDeviceOption> Scan() =>
        [.. LxiDiscovery.ScanAsync(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1)).GetAwaiter().GetResult().Select(LxiDeviceOption.FromDevice)];

    /// <summary>Every discovery probe (LXI, mDNS, SSDP, USR), for the Connection Editor's "Detect network devices..." picker: one row per device, its services as badges.</summary>
    public static IReadOnlyList<LxiDeviceOption> ScanNetwork() =>
        [.. NetworkDiscovery.GroupByDevice(NetworkDiscovery.CreateDefault().DiscoverAsync(TimeSpan.FromSeconds(3)).GetAwaiter().GetResult()).Select(LxiDeviceOption.FromDevice)];
}
