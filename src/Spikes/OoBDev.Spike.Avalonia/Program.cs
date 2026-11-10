using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace OoBDev.Spike.Avalonia;

/// <summary>Starts the demo window: <c>dotnet run --project src/Spikes/OoBDev.Spike.Avalonia</c>.</summary>
public static class Program
{
    /// <summary>Entry point.</summary>
    [STAThread]
    public static int Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    /// <summary>Builds the application for the desktop and for the headless tests.</summary>
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<SpikeApp>().UsePlatformDetect();
}
