using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Opens the converter tools window.</summary>
/// <param name="shell">The shell.</param>
internal sealed class ShowConverterToolsCommand(ShellViewModel shell) : CommandBase
{
    public override bool CanExecute(object? parameter) => shell.CanEditConverterTools;

    public override void Execute(object? parameter) => shell.EditConverterTools();
}
