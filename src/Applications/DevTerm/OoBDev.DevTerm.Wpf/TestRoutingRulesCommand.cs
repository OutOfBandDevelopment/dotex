using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Runs the sample line through every draft rule.</summary>
/// <param name="dialog">The routing dialog view model.</param>
internal sealed class TestRoutingRulesCommand(RoutingDialogViewModel dialog) : CommandBase
{
    public override void Execute(object? parameter) => dialog.TestRules();
}
