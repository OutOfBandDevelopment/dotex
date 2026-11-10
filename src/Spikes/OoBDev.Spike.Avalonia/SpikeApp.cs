using Avalonia;
using Avalonia.Themes.Fluent;

namespace OoBDev.Spike.Avalonia;

/// <summary>Minimal Avalonia application with the Fluent theme (no XAML).</summary>
public sealed class SpikeApp : Application
{
    /// <inheritdoc />
    public override void Initialize() => Styles.Add(new FluentTheme());
}
