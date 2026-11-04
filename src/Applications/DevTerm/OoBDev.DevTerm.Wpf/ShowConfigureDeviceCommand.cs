using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Shows the Configure device window.</summary>
/// <param name="shell">The shell.</param>
internal sealed class ShowConfigureDeviceCommand(ShellViewModel shell) : CommandBase
{
    public override bool CanExecute(object? parameter) => shell.CanConfigureDevice;

    public override void Execute(object? parameter) => shell.ConfigureDevice();
}
