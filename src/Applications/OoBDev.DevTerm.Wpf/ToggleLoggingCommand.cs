using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Starts or stops logging the session to its default log file.</summary>
/// <param name="session">The session view model.</param>
internal sealed class ToggleLoggingCommand(SessionViewModel session) : CommandBase
{
    public override bool CanExecute(object? parameter) => session.CanLog;

    public override void Execute(object? parameter) => session.SwitchLogging();
}
