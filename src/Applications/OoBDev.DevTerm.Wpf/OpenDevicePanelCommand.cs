using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Opens the device panel passed as the parameter for the selected session, when the connection suits it.</summary>
/// <param name="shell">The shell.</param>
internal sealed class OpenDevicePanelCommand(ShellViewModel shell) : CommandBase
{
    public override bool CanExecute(object? parameter) => parameter is IDevicePanelContribution panel && shell.DevicePanelUnavailableReason(panel) is null;

    public override void Execute(object? parameter)
    {
        if (parameter is IDevicePanelContribution panel && CanExecute(panel))
        {
            shell.OpenDevicePanel(panel);
        }
    }
}
