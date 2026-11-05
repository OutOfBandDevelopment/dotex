using System.IO.Ports;
using System.Net;
using System.Text.RegularExpressions;

namespace OoBDev.System.IO.Ports.Bridge;

public static partial class ConfigValidator
{
    public static IReadOnlyList<string> Validate(BridgeOptions bridge)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(bridge.ComPort))
        {
            errors.Add("ComPort is required.");
        }

        if (bridge.BaudRate <= 0)
        {
            errors.Add($"{bridge.ComPort}: BaudRate must be positive.");
        }

        if (bridge.DataBits is < 5 or > 8)
        {
            errors.Add($"{bridge.ComPort}: DataBits must be 5-8.");
        }

        if (!Enum.IsDefined(bridge.Parity))
        {
            errors.Add($"{bridge.ComPort}: Parity is not valid.");
        }

        if (bridge.StopBits is StopBits.None || !Enum.IsDefined(bridge.StopBits))
        {
            errors.Add($"{bridge.ComPort}: StopBits must be One, OnePointFive or Two.");
        }

        if (bridge.TcpPort is < 1 or > 65535)
        {
            errors.Add($"{bridge.ComPort}: TcpPort must be 1-65535.");
        }

        if (!IPAddress.TryParse(bridge.BindAddress, out _))
        {
            errors.Add($"{bridge.ComPort}: BindAddress '{bridge.BindAddress}' is not an IP address.");
        }

        if (!Enum.IsDefined(bridge.Protocol))
        {
            errors.Add($"{bridge.ComPort}: Protocol must be Raw or Rfc2217.");
        }

        if (bridge.Name is { Length: > 63 })
        {
            errors.Add($"{bridge.ComPort}: Name must be 63 characters or fewer.");
        }

        foreach (var service in bridge.MdnsServices ?? [])
        {
            if (!ServiceTypePattern().IsMatch(service.ServiceType ?? string.Empty))
            {
                errors.Add($"{bridge.ComPort}: mDNS service type '{service.ServiceType}' must look like _name._tcp or _name._udp.");
            }

            if (service.Port is < 1 or > 65535)
            {
                errors.Add($"{bridge.ComPort}: mDNS port must be 1-65535.");
            }

            if (service.Name is { Length: > 63 })
            {
                errors.Add($"{bridge.ComPort}: mDNS name must be 63 characters or fewer.");
            }
        }

        return errors;
    }

    [GeneratedRegex(@"^_[A-Za-z0-9][A-Za-z0-9-]{0,14}\._(tcp|udp)$")]
    private static partial Regex ServiceTypePattern();

    public static IReadOnlyList<string> Validate(IEnumerable<BridgeOptions> bridges)
    {
        var list = bridges.ToList();
        var errors = list.SelectMany(Validate).ToList();

        errors.AddRange(list
            .Where(b => !string.IsNullOrWhiteSpace(b.ComPort))
            .GroupBy(b => b.ComPort.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => $"{g.Key} is configured more than once."));

        errors.AddRange(list
            .GroupBy(b => b.TcpPort)
            .Where(g => g.Count() > 1)
            .Select(g => $"TCP port {g.Key} is used by more than one bridge."));

        return errors;
    }
}
