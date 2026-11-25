using Avalonia;
using Avalonia.Themes.Fluent;

namespace OoBDev.UiDefinitions.Avalonia.Tests;

/// <summary>Minimal headless application with the Fluent theme.</summary>
public sealed class TestApp : Application
{
    /// <inheritdoc />
    public override void Initialize() => Styles.Add(new FluentTheme());
}
