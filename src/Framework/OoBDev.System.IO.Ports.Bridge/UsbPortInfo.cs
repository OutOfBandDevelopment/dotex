namespace OoBDev.System.IO.Ports.Bridge;

/// <summary>A serial port that is present now, with the USB ids Windows reports for it (empty when it is not a USB device).</summary>
/// <param name="PortName">The COM name, for example <c>COM13</c>.</param>
/// <param name="VendorId">Four hex digits, upper case, or empty.</param>
/// <param name="ProductId">Four hex digits, upper case, or empty.</param>
/// <param name="SerialNumber">The device serial number, or empty when the device has none.</param>
public sealed record UsbPortInfo(string PortName, string VendorId, string ProductId, string SerialNumber)
{
    public override string ToString() =>
        VendorId.Length == 0
            ? PortName
            : $"{PortName} {VendorId}:{ProductId}{(SerialNumber.Length == 0 ? string.Empty : " serial " + SerialNumber)}";
}
