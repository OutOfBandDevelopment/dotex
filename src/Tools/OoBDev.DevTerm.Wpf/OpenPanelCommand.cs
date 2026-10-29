using System.Linq;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Opens the panel of the <see cref="IPanelFactory"/> passed as the parameter, when the shell knows it.</summary>
/// <param name="shell">The shell to open the panel in.</param>
internal sealed class OpenPanelCommand(ShellViewModel shell) : CommandBase
{
    public override bool CanExecute(object? parameter) => parameter is IPanelFactory factory && shell.Factories.Contains(factory);

    public override void Execute(object? parameter)
    {
        if (parameter is IPanelFactory factory && CanExecute(factory))
        {
            shell.SelectedPanel = factory.Create();
        }
    }
}
