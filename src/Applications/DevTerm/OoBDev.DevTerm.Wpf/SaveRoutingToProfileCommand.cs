using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Saves routing into the saved profile.</summary>
/// <param name="dialog">The routing dialog view model.</param>
internal sealed class SaveRoutingToProfileCommand(RoutingDialogViewModel dialog) : CommandBase
{
    public override bool CanExecute(object? parameter) => true;

    public override void Execute(object? parameter) => dialog.Save();
}
