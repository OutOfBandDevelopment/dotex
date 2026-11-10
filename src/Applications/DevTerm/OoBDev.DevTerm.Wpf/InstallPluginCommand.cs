using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Installs a plugin from a zip (Device &gt; Install plugin).</summary>
/// <param name="shell">The shell.</param>
internal sealed class InstallPluginCommand(ShellViewModel shell) : CommandBase
{
    public override bool CanExecute(object? parameter) => shell.CanInstallPlugin;

    public override void Execute(object? parameter) => shell.InstallPlugin();
}
