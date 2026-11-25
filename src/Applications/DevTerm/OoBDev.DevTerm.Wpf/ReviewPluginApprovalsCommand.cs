using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Approves waiting plugins and reviews the remembered approvals (Device &gt; Plugin approvals).</summary>
/// <param name="shell">The shell.</param>
internal sealed class ReviewPluginApprovalsCommand(ShellViewModel shell) : CommandBase
{
    public override bool CanExecute(object? parameter) => shell.CanInstallPlugin;

    public override void Execute(object? parameter) => shell.ReviewPlugins();
}
