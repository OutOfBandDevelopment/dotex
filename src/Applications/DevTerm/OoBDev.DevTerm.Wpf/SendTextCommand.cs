using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Runs send on a <see cref="SessionViewModel"/>.</summary>
/// <param name="session">The session view model.</param>
internal sealed class SendTextCommand(SessionViewModel session) : CommandBase
{
    public override bool CanExecute(object? parameter) => session.CanSend;

    public override async void Execute(object? parameter) => await session.SendAsync().ConfigureAwait(true);
}
