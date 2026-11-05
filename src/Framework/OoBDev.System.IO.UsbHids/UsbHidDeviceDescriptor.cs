namespace OoBDev.System.IO.UsbHids;

/// <summary>
/// One HID device as seen during discovery, e.g. for a "--listhiddevices" CLI action or a GUI device
/// picker. <paramref name="DevicePath"/> (below) is the OS's own per-connection instance path — unlike
/// <paramref name="SerialNumber"/>, every enumerated device has one (confirmed against a real Velleman
/// K8055: no serial descriptor and a blank product name, but a real, always-present device path), so
/// it's what actually tells two otherwise-identical devices (same VID/PID, no serial — the K8055's
/// board-address DIP switch makes this a real, not just theoretical, case) apart during one session.
/// Persisted alongside <paramref name="SerialNumber"/> as its own <c>CliOptions.DevicePath</c> field
/// (not folded into <paramref name="SerialNumber"/> — HidSharp's own matching only ever understands a
/// real serial descriptor, so a folded value could never be found again when actually opening the
/// device), used only as a fallback when <paramref name="SerialNumber"/> is blank, since it's tied to
/// which USB port a device is plugged into and isn't portable across ports/reconnects the way a real
/// serial number is.
/// </summary>
public sealed record UsbHidDeviceDescriptor(int VendorId, int ProductId, string? ProductName, string? SerialNumber, string DevicePath);
