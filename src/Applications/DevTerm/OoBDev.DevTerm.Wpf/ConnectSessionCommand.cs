using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Runs connect on a <see cref="SessionViewModel"/>.</summary>
/// <param name="session">The session view model.</param>
internal sealed class ConnectSessionCommand(SessionViewModel session) : CommandBase
{
    public override bool CanExecute(object? parameter) => session.CanConnect;

    public override async void Execute(object? parameter) => await session.ConnectAsync().ConfigureAwait(true);
}
