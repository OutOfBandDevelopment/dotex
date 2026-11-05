using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Stops routing.</summary>
/// <param name="dialog">The routing dialog view model.</param>
internal sealed class StopRoutingCommand(RoutingDialogViewModel dialog) : CommandBase
{
    public override bool CanExecute(object? parameter) => true;

    public override async void Execute(object? parameter) => await dialog.StopAsync().ConfigureAwait(true);
}
