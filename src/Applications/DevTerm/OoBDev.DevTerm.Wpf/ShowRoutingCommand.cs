using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Opens the routing window for the selected session.</summary>
/// <param name="shell">The shell.</param>
internal sealed class ShowRoutingCommand(ShellViewModel shell) : CommandBase
{
    public override bool CanExecute(object? parameter) => shell.CanShowRouting;

    public override void Execute(object? parameter) => shell.ShowRouting();
}
