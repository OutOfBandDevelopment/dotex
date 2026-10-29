using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using OoBDev.DevTerm.Wpf;
using OoBDev.Scpi;
using OoBDev.UiDefinitions.Wpf;

namespace DevTermApp;

/// <summary>The composition root: registers the panel factories and shows the shell.</summary>
public partial class App : Application, IDisposable
{
    private ScpiTcpExecutor? _scpi;

    /// <inheritdoc />
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var picker = new WpfColorPicker();
        var factories = new List<IPanelFactory> { new DemoPanelFactory(picker) };
        if (ScpiTarget(e.Args) is { } target)
        {
            _scpi = new ScpiTcpExecutor(target.Host, target.Port);
            factories.Add(new ScpiPanelFactory(target.Profile, _scpi.ExecuteAsync, picker));
        }

        new MainWindow { DataContext = new ShellViewModel(factories) }.Show();
    }

    /// <summary>Closes the instrument connection.</summary>
    public void Dispose()
    {
        _scpi?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    protected override void OnExit(ExitEventArgs e)
    {
        Dispose();
        base.OnExit(e);
    }

    /// <summary>Reads <c>--scpi host[:port] [profile name]</c>; the profile defaults to the generic one.</summary>
    private static (string Host, int Port, ScpiInstrumentProfile Profile)? ScpiTarget(string[] args)
    {
        var at = Array.IndexOf(args, "--scpi");
        if (at < 0 || at + 1 >= args.Length) return null;

        var address = args[at + 1].Split(':', 2);
        var port = address.Length == 2 && int.TryParse(address[1], global::System.Globalization.CultureInfo.InvariantCulture, out var parsed) ? parsed : 5025;
        var catalog = ScpiProfileCatalog.Create();
        var wanted = at + 2 < args.Length ? args[at + 2] : null;
        var profile = catalog.Profiles.FirstOrDefault(p => string.Equals(p.Name, wanted, StringComparison.OrdinalIgnoreCase)) ?? ScpiProfileCatalog.Generic;
        return (address[0], port, profile);
    }
}
