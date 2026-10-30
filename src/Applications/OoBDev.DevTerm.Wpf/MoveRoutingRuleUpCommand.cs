using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Moves the rule row passed as the parameter one place earlier.</summary>
/// <param name="dialog">The routing dialog view model.</param>
internal sealed class MoveRoutingRuleUpCommand(RoutingDialogViewModel dialog) : CommandBase
{
    public override void Execute(object? parameter) => dialog.MoveRule(parameter as RoutingRuleRow, -1);
}
