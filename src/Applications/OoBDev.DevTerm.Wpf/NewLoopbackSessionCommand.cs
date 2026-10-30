using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Opens a loopback session in the shell.</summary>
/// <param name="shell">The shell.</param>
internal sealed class NewLoopbackSessionCommand(ShellViewModel shell) : CommandBase
{
    public override bool CanExecute(object? parameter) => shell.CanCreateSessions;

    public override void Execute(object? parameter) => shell.AddLoopbackSession();
}
