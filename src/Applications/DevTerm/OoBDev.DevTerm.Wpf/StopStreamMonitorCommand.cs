using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Stops the stream monitor.</summary>
/// <param name="monitor">The monitor window view model.</param>
internal sealed class StopStreamMonitorCommand(StreamMonitorDialogViewModel monitor) : CommandBase
{
    public override bool CanExecute(object? parameter) => monitor.IsRunning;

    public override void Execute(object? parameter) => monitor.StopMonitor();
}
