using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Reloads the hit counts and the routed message history.</summary>
/// <param name="dialog">The routing dialog view model.</param>
internal sealed class RefreshRoutingCommand(RoutingDialogViewModel dialog) : CommandBase
{
    public override void Execute(object? parameter) => dialog.Refresh();
}
