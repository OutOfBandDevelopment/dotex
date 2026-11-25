using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Opens the device manifest editor.</summary>
/// <param name="shell">The shell.</param>
internal sealed class ShowManifestEditorCommand(ShellViewModel shell) : CommandBase
{
    public override bool CanExecute(object? parameter) => shell.CanEditManifest;

    public override void Execute(object? parameter) => shell.EditManifest();
}
