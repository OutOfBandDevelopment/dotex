using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Device > Plugins: lists the loaded plugins and their status.</summary>
internal sealed class ShowPluginsCommand(ShellViewModel shell) : CommandBase
{
    public override bool CanExecute(object? parameter) => shell.CanInstallPlugin;

    public override void Execute(object? parameter) => shell.ShowPluginList();
}
