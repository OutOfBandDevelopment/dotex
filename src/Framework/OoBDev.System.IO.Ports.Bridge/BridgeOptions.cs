using System.IO.Ports;

namespace OoBDev.System.IO.Ports.Bridge;

/// <summary>One serial device bound to one TCP port.</summary>
public sealed record BridgeOptions
{
    public string ComPort { get; init; } = "COM1";

    public int BaudRate { get; init; } = 115200;

    public int DataBits { get; init; } = 8;

    public Parity Parity { get; init; } = Parity.None;

    public StopBits StopBits { get; init; } = StopBits.One;

    public int TcpPort { get; init; } = 4000;

    /// <summary>Local address the TCP listener binds to. 0.0.0.0 lets containers reach it via host.docker.internal.</summary>
    public string BindAddress { get; init; } = "0.0.0.0";

    /// <summary>Raw TCP pass-through, or RFC 2217 (telnet COM-PORT-OPTION) so clients can change line settings.</summary>
    public BridgeProtocol Protocol { get; init; } = BridgeProtocol.Raw;

    /// <summary>Instance name used for mDNS advertisement. Defaults to the COM port name.</summary>
    public string? Name { get; init; }

    /// <summary>Extra mDNS announcements for this bridge, in addition to the default entry.</summary>
    public IList<MdnsService>? MdnsServices { get; init; }

    /// <summary>When true a new connection disconnects the current client instead of being refused (newest program wins).</summary>
    public bool ReplaceExistingClient { get; init; }

    /// <summary>USB vendor id in hex (for example <c>067B</c>). With <see cref="UsbProductId"/> it selects the port instead of <see cref="ComPort"/>, so a renumbered COM port is found again.</summary>
    public string? UsbVendorId { get; init; }

    public string? UsbProductId { get; init; }

    /// <summary>Needed only when two attached devices share the vendor and product id.</summary>
    public string? UsbSerialNumber { get; init; }

    public bool SelectsByUsbId => !string.IsNullOrWhiteSpace(UsbVendorId) && !string.IsNullOrWhiteSpace(UsbProductId);

    public string UsbDescription => $"{UsbVendorId?.Trim().ToUpperInvariant()}:{UsbProductId?.Trim().ToUpperInvariant()}{(string.IsNullOrWhiteSpace(UsbSerialNumber) ? string.Empty : " serial " + UsbSerialNumber.Trim())}";

    /// <summary>The port name used in logs: the COM name, or the USB id when the port is selected by it.</summary>
    public string PortLabel => SelectsByUsbId ? UsbDescription : ComPort;

    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? PortLabel : Name.Trim();

    public override string ToString() =>
        $"{PortLabel} {BaudRate} {DataBits}{Parity.ToString()[0]}{StopBitsText(StopBits)} <-> {BindAddress}:{TcpPort} ({Protocol})";

    private static string StopBitsText(StopBits stopBits) => stopBits switch
    {
        StopBits.One => "1",
        StopBits.OnePointFive => "1.5",
        StopBits.Two => "2",
        _ => "?",
    };
}
