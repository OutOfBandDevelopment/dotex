using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.DevTerm.Devices.Scpi;
using OoBDev.DevTerm.Panels;
using OoBDev.Kuando.Busylight.Panels;
using OoBDev.Velleman.K8055.Panels;
using OoBDev.Zoom.H4n.Panels;

namespace OoBDev.DevTerm.Devices;

/// <summary>Registers the device panels DevTerm ships with.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds the K8055, Busylight and Zoom H4n panels and the SCPI presenter and instrument panel.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same collection.</returns>
    public static IServiceCollection AddDevTermDevicePanels(this IServiceCollection services)
    {
        services.TryAddDevicePanel<K8055Panel>();
        services.TryAddDevicePanel<BusylightPanel>();
        services.TryAddDevicePanel<ZoomH4nPanel>();
        services.AddTransient<IPresenter, ScpiReplyPresenter>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IInstrumentPanelProvider, ScpiInstrumentPanelProvider>());
        return services;
    }
}
