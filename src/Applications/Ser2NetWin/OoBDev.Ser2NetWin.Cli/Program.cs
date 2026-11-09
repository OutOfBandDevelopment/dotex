using System.IO.Ports;
using Microsoft.Extensions.Hosting.WindowsServices;
using OoBDev.System.IO.Ports.Bridge;
using OoBDev.Ser2NetWin.Cli;
using Spectre.Console;

const string Usage = """
    ser2net-win - bridge Windows COM ports to raw TCP sockets

    Usage: ser2net-win [command] [--config <path>]

    Commands:
      run         Run the bridges in this console (default; Ctrl+C to stop)
      config      Open the interactive editor for port mappings
      list        Show the configured mappings
      ports       Show the COM ports currently present
      install     Register the Windows Service (admin; add --start to start it)
      uninstall   Stop and remove the Windows Service (admin)
      start       Start the installed service (admin)
      stop        Stop the installed service (admin)
      status      Show the service state

    Options:
      -c, --config <path>   JSON config file (default: appsettings.json next to the exe)
    """;

// The SCM launches the exe with the arguments baked into the service binPath; just run.
if (WindowsServiceHelpers.IsWindowsService())
{
    return await HostRunner.RunAsync(CliOptions.Parse(args).ResolvedConfigPath).ConfigureAwait(false);
}

var options = CliOptions.Parse(args);
if (options.Error is not null)
{
    Console.Error.WriteLine(options.Error);
    Console.Error.WriteLine("Run 'ser2net-win --help' for usage.");
    return 2;
}

if (options.Command is "install" or "uninstall" or "start" or "stop")
{
    if (!OperatingSystem.IsWindows())
    {
        Console.Error.WriteLine("Service management is only available on Windows.");
        return 1;
    }

    if (!ServiceInstaller.IsAdministrator())
    {
        Console.Error.WriteLine("This command must be run from an elevated (administrator) terminal.");
        return 1;
    }
}

string? ServiceCommand(string? error, string success)
{
    Console.WriteLine(error is null ? success : $"Failed: {error}");
    return error;
}

switch (options.Command)
{
    case "run":
        return await HostRunner.RunAsync(options.ResolvedConfigPath).ConfigureAwait(false);

    case "config":
        return new ConfigEditor(new ConfigStore(options.ResolvedConfigPath), AnsiConsole.Console).Run();

    case "list":
        var bridges = new ConfigStore(options.ResolvedConfigPath).Load().Bridges;
        Console.WriteLine($"{options.ResolvedConfigPath}: {bridges.Count} bridge(s)");
        foreach (var bridge in bridges)
        {
            Console.WriteLine($"  {bridge}");
        }

        return 0;

    case "ports":
        foreach (var port in SerialPort.GetPortNames().OrderBy(p => p.Length).ThenBy(p => p, StringComparer.Ordinal))
        {
            Console.WriteLine(port);
        }

        return 0;

    case "install":
        return ServiceCommand(ServiceInstaller.Install(options.ResolvedConfigPath, options.StartAfterInstall), "Service installed.") is null ? 0 : 1;

    case "uninstall":
        return ServiceCommand(ServiceInstaller.Uninstall(), "Service removed.") is null ? 0 : 1;

    case "start":
        return ServiceCommand(ServiceInstaller.Start(), "Service started.") is null ? 0 : 1;

    case "stop":
        return ServiceCommand(ServiceInstaller.Stop(), "Service stopped.") is null ? 0 : 1;

    case "status":
        Console.WriteLine($"{ServiceInstaller.ServiceName}: {ServiceInstaller.Status()}");
        return 0;

    case "help":
        Console.WriteLine(Usage);
        return 0;

    default:
        Console.Error.WriteLine($"Unknown command '{options.Command}'.");
        Console.Error.WriteLine(Usage);
        return 2;
}
