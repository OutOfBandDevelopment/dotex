using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Opens the stream monitor for the selected session.</summary>
/// <param name="shell">The shell.</param>
internal sealed class ShowStreamMonitorCommand(ShellViewModel shell) : CommandBase
{
    public override bool CanExecute(object? parameter) => shell.CanShowStreamMonitor;

    public override void Execute(object? parameter) => shell.ShowStreamMonitor();
}
