using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Clears the merged log.</summary>
/// <param name="log">The merged log window view model.</param>
internal sealed class ClearMergedLogCommand(MergedLogDialogViewModel log) : CommandBase
{
    public override bool CanExecute(object? parameter) => true;

    public override void Execute(object? parameter) => log.ClearLog();
}
