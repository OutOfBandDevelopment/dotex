using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Picks a session log and opens the playback window.</summary>
/// <param name="shell">The shell.</param>
internal sealed class OpenPlaybackCommand(ShellViewModel shell) : CommandBase
{
    public override bool CanExecute(object? parameter) => shell.CanOpenPlayback;

    public override void Execute(object? parameter) => shell.OpenPlayback();
}
