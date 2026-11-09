using System;
using System.Collections.Generic;

namespace OoBDev.System.IO.Ports;

public sealed class SystemSerialPortDiscovery : ISerialPortDiscovery
{
    public IReadOnlyList<string> GetPortNames() => global::System.IO.Ports.SerialPort.GetPortNames();

    /// <summary>
    /// Per OS: Windows reads the Plug-and-Play registry's friendly names
    /// (<see cref="WindowsSerialPortDescriptions"/>), Linux the USB string descriptors sysfs exposes
    /// (<see cref="LinuxSerialPortDescriptions"/>), macOS the IOKit registry via <c>ioreg</c>
    /// (<see cref="MacSerialPortDescriptions"/>). Anything else reports no descriptions, so the
    /// picker shows short names. Keys match what <see cref="GetPortNames"/> reports on that OS
    /// (<c>"COM3"</c>, <c>"/dev/ttyUSB0"</c>, <c>"/dev/cu.usbserial-A50285BI"</c>).
    /// </summary>
    public IReadOnlyDictionary<string, string> GetPortDescriptions()
    {
        if (OperatingSystem.IsWindows())
        {
            return WindowsSerialPortDescriptions.Read();
        }

        if (OperatingSystem.IsLinux())
        {
            return LinuxSerialPortDescriptions.Read();
        }

        if (OperatingSystem.IsMacOS())
        {
            return MacSerialPortDescriptions.Read();
        }

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Windows' friendly names embed the port — <c>"USB Serial Device (COM3)"</c> — which a picker
    /// already shows next to the description, so drop the trailing <c>(COMn)</c>. Returns the input
    /// (trimmed) if it doesn't end that way.
    /// </summary>
    public static string StripPortSuffix(string friendlyName, string portName)
    {
        ArgumentNullException.ThrowIfNull(friendlyName);
        ArgumentNullException.ThrowIfNull(portName);
        var trimmed = friendlyName.Trim();
        var suffix = $"({portName})";
        return trimmed.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
            ? trimmed[..^suffix.Length].TrimEnd()
            : trimmed;
    }
}
