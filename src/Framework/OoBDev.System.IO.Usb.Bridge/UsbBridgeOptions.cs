using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;

namespace OoBDev.System.IO.Usb.Bridge;

/// <summary>One USB device bound to one TCP port.</summary>
public sealed record UsbBridgeOptions
{
    /// <summary>USBTMC or HID.</summary>
    public UsbBridgeKind Kind { get; init; } = UsbBridgeKind.UsbTmc;

    /// <summary>USB vendor id in hex, for example <c>1AB1</c>.</summary>
    public string? UsbVendorId { get; init; }

    /// <summary>USB product id in hex, for example <c>09C4</c>.</summary>
    public string? UsbProductId { get; init; }

    /// <summary>Needed only when two attached devices share the vendor and product id.</summary>
    public string? UsbSerialNumber { get; init; }

    /// <summary>The local TCP port.</summary>
    public int TcpPort { get; init; } = 4100;

    /// <summary>The local address the listener binds to.</summary>
    public string BindAddress { get; init; } = "0.0.0.0";

    /// <summary>The instance name in logs; defaults to the USB ids.</summary>
    public string? Name { get; init; }

    /// <summary>When true a new connection disconnects the current client instead of being refused.</summary>
    public bool ReplaceExistingClient { get; init; }

    /// <summary>Device read timeout in milliseconds.</summary>
    public int ReadTimeoutMs { get; init; } = 1000;

    /// <summary>Device write timeout in milliseconds.</summary>
    public int WriteTimeoutMs { get; init; } = 5000;

    /// <summary>The vendor id as a number, or null when it is not valid hex.</summary>
    public int? VendorId => Parse(UsbVendorId);

    /// <summary>The product id as a number, or null when it is not valid hex.</summary>
    public int? ProductId => Parse(UsbProductId);

    /// <summary>The ids as text, for logs.</summary>
    public string UsbDescription =>
        $"{UsbVendorId?.Trim().ToUpperInvariant()}:{UsbProductId?.Trim().ToUpperInvariant()}{(string.IsNullOrWhiteSpace(UsbSerialNumber) ? string.Empty : " serial " + UsbSerialNumber)}";

    /// <summary>The name used in logs.</summary>
    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? $"{Kind} {UsbDescription}" : Name.Trim();

    /// <summary>The problems with the configuration, empty when it is valid.</summary>
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        if (VendorId is not (> 0 and <= 0xFFFF))
            errors.Add($"{DisplayName}: UsbVendorId must be a hex number up to FFFF.");
        if (ProductId is not (> 0 and <= 0xFFFF))
            errors.Add($"{DisplayName}: UsbProductId must be a hex number up to FFFF.");
        if (TcpPort is < 1 or > 65535)
            errors.Add($"{DisplayName}: TcpPort must be 1 to 65535.");
        if (!IPAddress.TryParse(BindAddress, out _))
            errors.Add($"{DisplayName}: BindAddress is not an IP address.");
        return errors;
    }

    private static int? Parse(string? hex) =>
        int.TryParse(hex?.Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value) ? value : null;
}
