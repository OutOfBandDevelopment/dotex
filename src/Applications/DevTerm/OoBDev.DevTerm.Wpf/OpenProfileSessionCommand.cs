using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Opens a session from the saved profile whose name is passed as the parameter.</summary>
/// <param name="shell">The shell.</param>
internal sealed class OpenProfileSessionCommand(ShellViewModel shell) : CommandBase
{
    public override bool CanExecute(object? parameter) => shell.CanCreateSessions && parameter is string { Length: > 0 };

    public override void Execute(object? parameter)
    {
        if (parameter is string name && CanExecute(name))
        {
            shell.AddProfileSession(name);
        }
    }
}
