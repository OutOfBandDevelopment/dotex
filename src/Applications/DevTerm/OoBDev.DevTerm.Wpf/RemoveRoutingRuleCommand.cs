using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Removes the rule row passed as the parameter.</summary>
/// <param name="dialog">The routing dialog view model.</param>
internal sealed class RemoveRoutingRuleCommand(RoutingDialogViewModel dialog) : CommandBase
{
    public override void Execute(object? parameter) => dialog.RemoveRule(parameter as RoutingRuleRow);
}
