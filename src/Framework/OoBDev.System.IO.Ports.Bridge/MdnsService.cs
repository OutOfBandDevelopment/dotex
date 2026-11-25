namespace OoBDev.System.IO.Ports.Bridge;

/// <summary>
/// An extra mDNS / DNS-SD announcement for a bridge, advertised in addition to the default
/// <c>_ser2net._tcp</c> entry (for example a second service type another tool browses for).
/// </summary>
public sealed record MdnsService
{
    /// <summary>DNS-SD service type, such as <c>_zwave-js-server._tcp</c>.</summary>
    public string ServiceType { get; init; } = string.Empty;

    /// <summary>Instance name; defaults to the bridge's name.</summary>
    public string? Name { get; init; }

    /// <summary>Port to announce; defaults to the bridge's TCP port.</summary>
    public int? Port { get; init; }

    /// <summary>TXT record entries. When omitted the default protocol/port/baud entries are used.</summary>
    public IDictionary<string, string>? Txt { get; init; }
}
