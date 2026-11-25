using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Picks a device manifest and opens its panel on the selected session.</summary>
/// <param name="shell">The shell.</param>
internal sealed class OpenManifestCommand(ShellViewModel shell) : CommandBase
{
    public override bool CanExecute(object? parameter) => shell.CanOpenManifest;

    public override void Execute(object? parameter) => shell.OpenManifest();
}
