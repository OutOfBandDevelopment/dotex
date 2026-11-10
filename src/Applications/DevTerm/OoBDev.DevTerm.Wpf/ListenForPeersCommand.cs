using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Starts listening for TCP peers, one session tab per peer.</summary>
/// <param name="shell">The shell.</param>
internal sealed class ListenForPeersCommand(ShellViewModel shell) : CommandBase
{
    public override bool CanExecute(object? parameter) => shell.CanListenForPeers;

    public override void Execute(object? parameter) => shell.StartListeningForPeers();
}
