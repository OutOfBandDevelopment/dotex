using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;

namespace OoBDev.Spike.Avalonia;

/// <summary>Minimal Avalonia application with the Fluent theme (no XAML) that opens the demo form.</summary>
public sealed class SpikeApp : Application
{
    /// <inheritdoc />
    public override void Initialize() => Styles.Add(new FluentTheme());

    /// <inheritdoc />
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = DemoForm.CreateWindow(new DemoModel());
        }

        base.OnFrameworkInitializationCompleted();
    }
}
