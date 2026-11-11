using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;

namespace OoBDev.Examples.Controls.Avalonia;

/// <summary>Minimal Avalonia application with the Fluent theme (no XAML) that opens the generated settings form.</summary>
public sealed class ControlsApp : Application
{
    /// <inheritdoc />
    public override void Initialize() => Styles.Add(new FluentTheme());

    /// <inheritdoc />
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = ControlsWindow.Build(new ControlsSampleSettings(), out _);
            new LiveSample().CreateWindow(run: true).Show();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
