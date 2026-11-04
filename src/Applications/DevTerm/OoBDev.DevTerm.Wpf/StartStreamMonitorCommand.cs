using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Starts the stream monitor.</summary>
/// <param name="monitor">The monitor window view model.</param>
internal sealed class StartStreamMonitorCommand(StreamMonitorDialogViewModel monitor) : CommandBase
{
    public override bool CanExecute(object? parameter) => !monitor.IsRunning;

    public override void Execute(object? parameter) => monitor.StartMonitor();
}
