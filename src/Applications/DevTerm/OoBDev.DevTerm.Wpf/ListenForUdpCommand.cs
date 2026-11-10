using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Binds a UDP port and opens a session tab that shows each datagram's sender.</summary>
/// <param name="shell">The shell.</param>
internal sealed class ListenForUdpCommand(ShellViewModel shell) : CommandBase
{
    public override bool CanExecute(object? parameter) => shell.CanListenForUdp;

    public override void Execute(object? parameter) => shell.StartListeningForUdp();
}
