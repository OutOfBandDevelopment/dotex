using OoBDev.System.IO.Ports.Bridge;
using System.IO.Ports;
using System.Net;
using Spectre.Console;

namespace OoBDev.Ser2NetWin.Cli;

/// <summary>Interactive terminal UI for editing the list of port mappings.</summary>
public sealed class ConfigEditor
{
    private const string Manual = "(type a name...)";
    private const string Custom = "(custom...)";

    private static readonly int[] CommonBaudRates = [1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200, 230400, 460800, 921600];

    private readonly ConfigStore _store;
    private readonly IAnsiConsole _console;

    public ConfigEditor(ConfigStore store, IAnsiConsole console)
    {
        _store = store;
        _console = console;
    }

    public int Run()
    {
        if (!_console.Profile.Capabilities.Interactive)
        {
            _console.MarkupLine("[red]The config editor needs an interactive terminal.[/] Edit the Bridges section of the JSON file directly instead.");
            return 1;
        }

        var config = _store.Load();
        var bridges = config.Bridges.ToList();
        var dirty = false;

        while (true)
        {
            Render(bridges, config.Mdns, dirty);

            var choices = new List<string> { "Add bridge" };
            if (bridges.Count > 0)
            {
                choices.Add("Edit bridge");
                choices.Add("Remove bridge");
            }

            choices.Add(config.Mdns.Enabled ? "Disable mDNS advertisement" : "Enable mDNS advertisement");
            choices.Add("Save and exit");
            choices.Add("Exit without saving");

            var action = _console.Prompt(new SelectionPrompt<string>().Title("What next?").AddChoices(choices));

            switch (action)
            {
                case "Add bridge":
                    var added = Edit(null, bridges);
                    if (added is not null)
                    {
                        bridges.Add(added);
                        dirty = true;
                    }

                    break;
                case "Edit bridge":
                    var index = PickIndex(bridges, "Edit which bridge?");
                    if (index >= 0)
                    {
                        var edited = Edit(bridges[index], bridges.Where((_, i) => i != index).ToList());
                        if (edited is not null)
                        {
                            bridges[index] = edited;
                            dirty = true;
                        }
                    }

                    break;
                case "Remove bridge":
                    var removeIndex = PickIndex(bridges, "Remove which bridge?");
                    if (removeIndex >= 0 && _console.Confirm($"Remove {bridges[removeIndex]}?", false))
                    {
                        bridges.RemoveAt(removeIndex);
                        dirty = true;
                    }

                    break;
                case "Enable mDNS advertisement":
                case "Disable mDNS advertisement":
                    config.Mdns.Enabled = !config.Mdns.Enabled;
                    dirty = true;
                    break;
                case "Save and exit":
                    var errors = ConfigValidator.Validate(bridges);
                    if (errors.Count > 0)
                    {
                        ShowErrors(errors);
                        _console.Prompt(new TextPrompt<string>("Press Enter to continue").AllowEmpty());
                        break;
                    }

                    config.Bridges = bridges;
                    _store.Save(config);
                    _console.MarkupLine($"[green]Saved[/] {Markup.Escape(_store.FilePath)}");
                    OfferServiceRestart();
                    return 0;
                default:
                    if (!dirty || _console.Confirm("Discard unsaved changes?", false))
                    {
                        return 0;
                    }

                    break;
            }
        }
    }

    private void Render(List<BridgeOptions> bridges, MdnsOptions mdns, bool dirty)
    {
        _console.Clear();
        _console.Write(new Rule("[bold]ser2net-win configuration[/]").LeftJustified());
        _console.MarkupLine($"[grey]{Markup.Escape(_store.FilePath)}{(dirty ? " (unsaved changes)" : string.Empty)}[/]");
        _console.WriteLine();

        var table = new Table().Border(TableBorder.Rounded);
        foreach (var header in new[] { "COM port", "Baud", "Data", "Parity", "Stop", "Protocol", "Listen" })
        {
            table.AddColumn(header);
        }

        foreach (var b in bridges)
        {
            table.AddRow(
                Markup.Escape(b.ComPort),
                b.BaudRate.ToString(),
                b.DataBits.ToString(),
                b.Parity.ToString(),
                b.StopBits.ToString(),
                b.Protocol.ToString(),
                $"{b.BindAddress}:{b.TcpPort}");
        }

        if (bridges.Count == 0)
        {
            table.AddRow("[grey]no bridges configured[/]", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        }

        _console.Write(table);
        _console.MarkupLine(mdns.Enabled ? $"[green]mDNS advertisement on[/] ({Markup.Escape(mdns.ServiceType)})" : "[grey]mDNS advertisement off[/]");
        _console.WriteLine();
    }

    private int PickIndex(List<BridgeOptions> bridges, string title)
    {
        var cancel = bridges.Count;
        var picked = _console.Prompt(new SelectionPrompt<int>()
            .Title(title)
            .AddChoices(Enumerable.Range(0, bridges.Count + 1))
            .UseConverter(i => i == cancel ? "(cancel)" : Markup.Escape(bridges[i].ToString())));
        return picked == cancel ? -1 : picked;
    }

    private BridgeOptions? Edit(BridgeOptions? existing, List<BridgeOptions> others)
    {
        var current = existing ?? new BridgeOptions { TcpPort = NextFreePort(others) };

        while (true)
        {
            var comPort = PickComPort(current.ComPort);
            var baud = PickBaud(current.BaudRate);
            var dataBits = Pick("Data bits", new[] { 8, 7, 6, 5 }, current.DataBits);
            var parity = Pick("Parity", Enum.GetValues<Parity>(), current.Parity);
            var stopBits = Pick("Stop bits", new[] { StopBits.One, StopBits.OnePointFive, StopBits.Two }, current.StopBits);
            var protocol = Pick("Protocol (Raw = plain TCP, Rfc2217 = clients may change line settings)", Enum.GetValues<BridgeProtocol>(), current.Protocol);
            var tcpPort = _console.Prompt(new TextPrompt<int>("TCP port to listen on:")
                .DefaultValue(current.TcpPort)
                .Validate(p => p is >= 1 and <= 65535 ? ValidationResult.Success() : ValidationResult.Error("Enter a port from 1 to 65535.")));
            var bind = _console.Prompt(new TextPrompt<string>("Bind address (0.0.0.0 = all interfaces, 127.0.0.1 = local only):")
                .DefaultValue(current.BindAddress)
                .Validate(a => IPAddress.TryParse(a, out _) ? ValidationResult.Success() : ValidationResult.Error("Enter an IP address.")));

            var replace = _console.Confirm("Replace the connected client when a new one connects (newest wins)?", current.ReplaceExistingClient);
            var name = _console.Prompt(new TextPrompt<string>("mDNS name (blank = COM port name):").DefaultValue(current.Name ?? string.Empty).AllowEmpty()).Trim();

            var services = EditServices(current.MdnsServices);

            var candidate = new BridgeOptions
            {
                ComPort = comPort,
                BaudRate = baud,
                DataBits = dataBits,
                Parity = parity,
                StopBits = stopBits,
                TcpPort = tcpPort,
                BindAddress = bind,
                Protocol = protocol,
                ReplaceExistingClient = replace,
                Name = name.Length == 0 ? null : name,
                MdnsServices = services.Count == 0 ? null : services,
            };

            var errors = ConfigValidator.Validate(others.Append(candidate));
            if (errors.Count == 0)
            {
                return candidate;
            }

            ShowErrors(errors);
            if (!_console.Confirm("Try again?", true))
            {
                return null;
            }

            current = candidate;
        }
    }

    private List<MdnsService> EditServices(List<MdnsService>? existing)
    {
        var services = existing?.ToList() ?? [];
        if (!_console.Confirm(services.Count == 0 ? "Add secondary mDNS entries?" : $"Edit the {services.Count} secondary mDNS entries?", false))
        {
            return services;
        }

        while (true)
        {
            var choices = new List<string> { "Add entry" };
            if (services.Count > 0)
            {
                choices.Add("Remove entry");
            }

            choices.Add("Done");
            _console.MarkupLine(services.Count == 0
                ? "[grey]No secondary entries.[/]"
                : string.Join(Environment.NewLine, services.Select(s => $"[grey]- {Markup.Escape(s.ServiceType)} name={Markup.Escape(s.Name ?? "(bridge)")} port={(s.Port?.ToString() ?? "(bridge)")}[/]")));

            switch (_console.Prompt(new SelectionPrompt<string>().Title("Secondary mDNS entries:").AddChoices(choices)))
            {
                case "Add entry":
                    var type = _console.Prompt(new TextPrompt<string>("Service type (for example _zwave-js-server._tcp):")
                        .Validate(t => ConfigValidator.Validate(new BridgeOptions { MdnsServices = [new MdnsService { ServiceType = t }] }).Count == 0
                            ? ValidationResult.Success()
                            : ValidationResult.Error("Use the form _name._tcp or _name._udp.")));
                    var entryName = _console.Prompt(new TextPrompt<string>("Instance name (blank = bridge name):").AllowEmpty()).Trim();
                    var port = _console.Prompt(new TextPrompt<int>("Port to announce (0 = bridge TCP port):").DefaultValue(0)
                        .Validate(p => p is >= 0 and <= 65535 ? ValidationResult.Success() : ValidationResult.Error("Enter 0-65535.")));
                    var txt = _console.Prompt(new TextPrompt<string>("TXT entries as key=value, comma separated (blank = default protocol/port/baud):").AllowEmpty()).Trim();
                    services.Add(new MdnsService
                    {
                        ServiceType = type.Trim(),
                        Name = entryName.Length == 0 ? null : entryName,
                        Port = port == 0 ? null : port,
                        Txt = ParseTxt(txt),
                    });
                    break;
                case "Remove entry":
                    var picked = _console.Prompt(new SelectionPrompt<int>()
                        .Title("Remove which entry?")
                        .AddChoices(Enumerable.Range(0, services.Count))
                        .UseConverter(i => Markup.Escape(services[i].ServiceType)));
                    services.RemoveAt(picked);
                    break;
                default:
                    return services;
            }
        }
    }

    private static Dictionary<string, string>? ParseTxt(string text)
    {
        if (text.Length == 0)
        {
            return null;
        }

        var result = new Dictionary<string, string>();
        foreach (var pair in text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var index = pair.IndexOf('=');
            if (index > 0)
            {
                result[pair[..index].Trim()] = pair[(index + 1)..].Trim();
            }
        }

        return result.Count == 0 ? null : result;
    }

    private string PickComPort(string current)
    {
        var ports = SerialPort.GetPortNames()
            .OrderBy(p => p.Length)
            .ThenBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (!ports.Contains(current, StringComparer.OrdinalIgnoreCase))
        {
            ports.Insert(0, current);
        }

        ports.Add(Manual);
        var picked = _console.Prompt(new SelectionPrompt<string>().Title("COM port:").AddChoices(ports).UseConverter(Markup.Escape));
        return picked == Manual ? _console.Prompt(new TextPrompt<string>("COM port name:").DefaultValue(current)).Trim() : picked;
    }

    private int PickBaud(int current)
    {
        var rates = CommonBaudRates.ToList();
        if (!rates.Contains(current))
        {
            rates.Insert(0, current);
        }

        var customMarker = -1;
        rates.Add(customMarker);
        var picked = _console.Prompt(new SelectionPrompt<int>()
            .Title("Baud rate:")
            .AddChoices(rates)
            .UseConverter(r => r == customMarker ? Custom : r.ToString()));
        return picked == customMarker
            ? _console.Prompt(new TextPrompt<int>("Baud rate:").DefaultValue(current).Validate(r => r > 0))
            : picked;
    }

    private T Pick<T>(string title, IEnumerable<T> choices, T current)
        where T : notnull
    {
        return _console.Prompt(new SelectionPrompt<T>()
            .Title($"{title}:")
            .AddChoices(choices.OrderBy(c => Equals(c, current) ? 0 : 1))
            .UseConverter(c => Equals(c, current) ? $"{c} (current)" : c.ToString() ?? string.Empty));
    }

    private static int NextFreePort(List<BridgeOptions> others) =>
        others.Count == 0 ? 4000 : others.Max(b => b.TcpPort) + 1;

    private void ShowErrors(IEnumerable<string> errors)
    {
        foreach (var error in errors)
        {
            _console.MarkupLine($"[red]- {Markup.Escape(error)}[/]");
        }
    }

    private void OfferServiceRestart()
    {
        if (!OperatingSystem.IsWindows() || !ServiceInstaller.Exists() || !ServiceInstaller.IsRunning())
        {
            return;
        }

        if (_console.Confirm($"The {ServiceInstaller.ServiceName} service is running. Restart it to apply the changes?", true))
        {
            var error = ServiceInstaller.Restart();
            _console.MarkupLine(error is null ? "[green]Service restarted.[/]" : $"[red]{Markup.Escape(error)}[/]");
        }
    }
}
