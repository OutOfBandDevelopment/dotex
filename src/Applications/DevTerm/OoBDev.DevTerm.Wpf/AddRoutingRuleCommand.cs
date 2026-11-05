using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Adds an empty device to broker rule to the draft.</summary>
/// <param name="dialog">The routing dialog view model.</param>
internal sealed class AddRoutingRuleCommand(RoutingDialogViewModel dialog) : CommandBase
{
    public override void Execute(object? parameter) => dialog.AddRule();
}
