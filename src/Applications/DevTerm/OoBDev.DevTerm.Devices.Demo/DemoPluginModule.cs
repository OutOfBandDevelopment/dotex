using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.DevTerm.Devices.Demo;

/// <summary>An example plugin: contributes a control panel for the loopback device (plugins/demo).</summary>
public sealed class DemoPluginModule : IPluginModule
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services) => services.AddSingleton<IDevicePanelContribution, DemoPanelContribution>();
}
