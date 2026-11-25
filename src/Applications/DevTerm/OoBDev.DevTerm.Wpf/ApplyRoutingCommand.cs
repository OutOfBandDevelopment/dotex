using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Applies the routing draft.</summary>
/// <param name="dialog">The routing dialog view model.</param>
internal sealed class ApplyRoutingCommand(RoutingDialogViewModel dialog) : CommandBase
{
    public override bool CanExecute(object? parameter) => true;

    public override async void Execute(object? parameter) => await dialog.ApplyAsync().ConfigureAwait(true);
}
