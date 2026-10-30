using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.DevTerm.Devices;
using OoBDev.DevTerm.Hosting;
using OoBDev.UiDefinitions.ViewModels;
using UiDefinitionsWpf;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Registers the WPF shell on top of the DevTerm core and the device panels.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds the core, the device panels, the colour picker, the demo panel factory and the shell view model.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same collection.</returns>
    public static IServiceCollection AddDevTermWpfShell(this IServiceCollection services)
    {
        services.AddDevTermCore();
        services.AddDevTermDevicePanels();
        services.TryAddSingleton<IColorPicker, WpfColorPicker>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IPanelFactory, DemoPanelFactory>());
        services.TryAddSingleton<ShellViewModel>();
        return services;
    }
}
