using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Starts routing.</summary>
/// <param name="dialog">The routing dialog view model.</param>
internal sealed class StartRoutingCommand(RoutingDialogViewModel dialog) : CommandBase
{
    public override bool CanExecute(object? parameter) => true;

    public override void Execute(object? parameter) => dialog.StartRouting();
}
