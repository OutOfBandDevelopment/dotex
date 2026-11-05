using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Opens the merged log window.</summary>
/// <param name="shell">The shell.</param>
internal sealed class ShowMergedLogCommand(ShellViewModel shell) : CommandBase
{
    public override bool CanExecute(object? parameter) => shell.CanShowMergedLog;

    public override void Execute(object? parameter) => shell.ShowMergedLog();
}
