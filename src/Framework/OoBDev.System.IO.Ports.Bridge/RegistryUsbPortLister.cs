using System.IO.Ports;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace OoBDev.System.IO.Ports.Bridge;

/// <summary>
/// Reads the USB ids from <c>HKLM\SYSTEM\CurrentControlSet\Enum\USB</c>. The registry keeps entries for unplugged devices,
/// so a port counts only when <see cref="SerialPort.GetPortNames"/> also lists it. Off Windows the ports have no ids.
/// </summary>
public sealed partial class RegistryUsbPortLister : IUsbPortLister
{
    private const string UsbKey = @"SYSTEM\CurrentControlSet\Enum\USB";

    public IReadOnlyList<UsbPortInfo> List()
    {
        var present = SerialPort.GetPortNames().ToHashSet(StringComparer.OrdinalIgnoreCase);
        var known = OperatingSystem.IsWindows() ? ReadRegistry() : [];
        var result = new List<UsbPortInfo>();

        foreach (var name in present.OrderBy(p => p.Length).ThenBy(p => p, StringComparer.Ordinal))
        {
            result.Add(known.FirstOrDefault(k => k.PortName.Equals(name, StringComparison.OrdinalIgnoreCase)) ?? new UsbPortInfo(name, string.Empty, string.Empty, string.Empty));
        }

        return result;
    }

    [global::System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static List<UsbPortInfo> ReadRegistry()
    {
        var found = new List<UsbPortInfo>();
        using var usb = Registry.LocalMachine.OpenSubKey(UsbKey);
        if (usb is null)
        {
            return found;
        }

        foreach (var device in usb.GetSubKeyNames())
        {
            var match = DeviceKeyPattern().Match(device);
            if (!match.Success)
            {
                continue;
            }

            using var deviceKey = usb.OpenSubKey(device);
            foreach (var instance in deviceKey?.GetSubKeyNames() ?? [])
            {
                using var parameters = deviceKey!.OpenSubKey(instance + @"\Device Parameters");
                if (parameters?.GetValue("PortName") is not string portName || portName.Length == 0)
                {
                    continue;
                }

                // Windows invents an instance id with '&' when the device has no serial number.
                var serial = instance.Contains('&', StringComparison.Ordinal) ? string.Empty : instance;
                found.Add(new UsbPortInfo(portName, match.Groups["vid"].Value.ToUpperInvariant(), match.Groups["pid"].Value.ToUpperInvariant(), serial));
            }
        }

        return found;
    }

    [GeneratedRegex(@"^VID_(?<vid>[0-9A-Fa-f]{4})&PID_(?<pid>[0-9A-Fa-f]{4})", RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 250)]
    private static partial Regex DeviceKeyPattern();
}
