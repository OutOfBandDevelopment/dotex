using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Picks a seed theme and edits a new theme from it.</summary>
/// <param name="shell">The shell.</param>
internal sealed class BuildThemeCommand(ShellViewModel shell) : CommandBase
{
    public override bool CanExecute(object? parameter) => shell.CanBuildTheme;

    public override void Execute(object? parameter) => shell.BuildThemeFromSeed();
}
