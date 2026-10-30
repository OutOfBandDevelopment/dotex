using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Opens the connection editor from the shell.</summary>
/// <param name="shell">The shell.</param>
internal sealed class NewConnectionCommand(ShellViewModel shell) : CommandBase
{
    public override bool CanExecute(object? parameter) => shell.CanEditConnections;

    public override void Execute(object? parameter) => shell.EditConnection();
}
