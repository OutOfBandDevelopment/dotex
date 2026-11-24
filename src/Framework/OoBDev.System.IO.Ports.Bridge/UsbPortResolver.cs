namespace OoBDev.System.IO.Ports.Bridge;

/// <summary>Picks the COM name a bridge should open: the configured name, or the one port that matches the configured USB ids.</summary>
public static class UsbPortResolver
{
    public static string Resolve(BridgeOptions options, IReadOnlyList<UsbPortInfo> ports)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(ports);

        if (!options.SelectsByUsbId)
        {
            return options.ComPort;
        }

        var matches = ports.Where(p => Matches(options, p)).ToList();
        return matches.Count switch
        {
            1 => matches[0].PortName,
            0 => throw new IOException($"No serial port with USB id {options.UsbDescription} is present."),
            _ => throw new InvalidOperationException($"More than one serial port matches USB id {options.UsbDescription}; set UsbSerialNumber to choose: {string.Join("; ", matches)}."),
        };
    }

    private static bool Matches(BridgeOptions options, UsbPortInfo port) =>
        port.VendorId.Equals(Normalize(options.UsbVendorId), StringComparison.OrdinalIgnoreCase)
        && port.ProductId.Equals(Normalize(options.UsbProductId), StringComparison.OrdinalIgnoreCase)
        && (string.IsNullOrWhiteSpace(options.UsbSerialNumber) || port.SerialNumber.Equals(options.UsbSerialNumber.Trim(), StringComparison.OrdinalIgnoreCase));

    private static string Normalize(string? id) => (id ?? string.Empty).Trim().TrimStart('0', 'x', 'X').PadLeft(4, '0');
}
