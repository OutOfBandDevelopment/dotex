using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Applies the theme named by <paramref name="name"/>.</summary>
/// <param name="themes">The theme choice.</param>
/// <param name="name">The theme to apply.</param>
internal sealed class SelectThemeCommand(ThemeViewModel themes, string name) : CommandBase
{
    public override void Execute(object? parameter) => themes.Selected = name;
}
