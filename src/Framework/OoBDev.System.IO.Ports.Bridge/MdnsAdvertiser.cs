using System.Globalization;
using Makaretu.Dns;

namespace OoBDev.System.IO.Ports.Bridge;

/// <summary>
/// Advertises bridges over mDNS / DNS-SD: the default service type plus any extra entries configured on the bridge.
/// </summary>
public sealed class MdnsAdvertiser : IServiceAdvertiser, IDisposable
{
    private readonly MdnsOptions _options;
    private readonly Lock _gate = new();
    private MulticastService? _mdns;
    private ServiceDiscovery? _discovery;

    public MdnsAdvertiser(MdnsOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Resolves every announcement for a bridge (default entry first) with name, port and TXT values filled in.
    /// </summary>
    public static IReadOnlyList<MdnsService> Plan(BridgeOptions bridge, int port, string defaultServiceType)
    {
        var defaultTxt = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["protocol"] = bridge.Protocol == BridgeProtocol.Rfc2217 ? "rfc2217" : "raw",
            ["port"] = bridge.ComPort,
            ["baud"] = bridge.BaudRate.ToString(CultureInfo.InvariantCulture),
        };

        var entries = new List<MdnsService>
        {
            new() { ServiceType = defaultServiceType, Name = bridge.DisplayName, Port = port, Txt = defaultTxt },
        };

        foreach (var extra in bridge.MdnsServices ?? [])
        {
            entries.Add(extra with
            {
                Name = string.IsNullOrWhiteSpace(extra.Name) ? bridge.DisplayName : extra.Name.Trim(),
                Port = extra.Port ?? port,
                Txt = extra.Txt ?? defaultTxt,
            });
        }

        return entries;
    }

    public IDisposable Advertise(BridgeOptions bridge, int port)
    {
        var discovery = GetDiscovery();
        var profiles = new List<ServiceProfile>();

        foreach (var entry in Plan(bridge, port, _options.ServiceType))
        {
            var profile = new ServiceProfile(entry.Name!, entry.ServiceType, (ushort)entry.Port!.Value);
            foreach (var (key, value) in entry.Txt!)
            {
                profile.AddProperty(key, value);
            }

            discovery.Advertise(profile);
            discovery.Announce(profile);
            profiles.Add(profile);
        }

        return new ActionDisposable(() =>
        {
            foreach (var profile in profiles)
            {
                discovery.Unadvertise(profile);
            }
        });
    }

    public void Dispose()
    {
        lock (_gate)
        {
            _discovery?.Dispose();
            _mdns?.Stop();
            _mdns?.Dispose();
            _discovery = null;
            _mdns = null;
        }
    }

    private ServiceDiscovery GetDiscovery()
    {
        lock (_gate)
        {
            if (_discovery is null)
            {
                _mdns = new MulticastService();
                _discovery = new ServiceDiscovery(_mdns);
                _mdns.Start();
            }

            return _discovery;
        }
    }
}
