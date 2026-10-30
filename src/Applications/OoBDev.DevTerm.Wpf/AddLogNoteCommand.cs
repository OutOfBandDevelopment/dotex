using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Adds a timestamped note to the running session log.</summary>
/// <param name="session">The session view model.</param>
internal sealed class AddLogNoteCommand(SessionViewModel session) : CommandBase
{
    public override bool CanExecute(object? parameter) => session.IsLogging;

    public override void Execute(object? parameter) => session.AddNote();
}
