using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.Devices;
using OoBDev.DevTerm.Hosting;
using OoBDev.DevTerm.Loopback;
using OoBDev.DevTerm.Presenters.Text;
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
        services.TryAddSingleton<IConfirmPrompt, WpfConfirmPrompt>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IPanelFactory, DemoPanelFactory>());
        services.TryAddTextPresenters();
        services.TryAddLoopbackTransport();
        services.TryAddSingleton(_ => new ConnectionProfileStore());
        services.TryAddSingleton<IPlaybackDialog, WpfPlaybackDialog>();
        services.TryAddSingleton(_ => new PlaybackPresenters());
        services.TryAddSingleton<IMergedLogDialog, WpfMergedLogDialog>();
        services.TryAddSingleton(_ => new MergedSessionLog());
        services.TryAddSingleton<IStreamMonitorDialog, WpfStreamMonitorDialog>();
        services.TryAddSingleton(_ => new StreamMonitor());
        services.TryAddSingleton<IRoutingDialog, WpfRoutingDialog>();
        services.TryAddSingleton<IConverterToolsDialog, WpfConverterToolsDialog>();
        services.TryAddSingleton<IManifestPickerDialog, WpfManifestPickerDialog>();
        services.TryAddSingleton<IThemeBuilderDialog, WpfThemeBuilderDialog>();
        services.TryAddSingleton<IConfigureDeviceDialog, WpfConfigureDeviceDialog>();
        services.TryAddSingleton(_ => new ConverterToolsStore());
        services.TryAddSingleton<IConnectionEditorDialog, WpfConnectionEditorDialog>();
        services.TryAddSingleton<ISessionViewModelFactory, SessionViewModelFactory>();
        services.TryAddSingleton<ShellViewModel>();
        return services;
    }
}
