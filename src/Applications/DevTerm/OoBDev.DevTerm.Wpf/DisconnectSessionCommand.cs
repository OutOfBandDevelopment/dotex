using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Runs disconnect on a <see cref="SessionViewModel"/>.</summary>
/// <param name="session">The session view model.</param>
internal sealed class DisconnectSessionCommand(SessionViewModel session) : CommandBase
{
    public override bool CanExecute(object? parameter) => session.CanDisconnect;

    public override async void Execute(object? parameter) => await session.DisconnectAsync().ConfigureAwait(true);
}
