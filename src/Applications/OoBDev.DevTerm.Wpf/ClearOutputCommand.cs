using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Empties the output of a <see cref="SessionViewModel"/>.</summary>
/// <param name="session">The session view model.</param>
internal sealed class ClearOutputCommand(SessionViewModel session) : CommandBase
{
    public override void Execute(object? parameter) => session.OutputLines.Clear();
}
