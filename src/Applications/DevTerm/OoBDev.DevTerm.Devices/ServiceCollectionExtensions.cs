using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.DerEE.De5000.Panels;
using OoBDev.DevTerm.Devices.Scpi;
using OoBDev.DevTerm.Panels;
using OoBDev.Kuando.Busylight.Panels;
using OoBDev.LanC.Panels;
using OoBDev.Quarta.RadexOne.Panels;
using OoBDev.Velleman.K8055.Panels;
using OoBDev.Zoom.H4n.Panels;

namespace OoBDev.DevTerm.Devices;

/// <summary>Registers the device panels DevTerm ships with.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds the K8055, Busylight, Radex One, DE-5000, LANC and Zoom H4n panels and the SCPI presenter and instrument panel.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same collection.</returns>
    public static IServiceCollection AddDevTermDevicePanels(this IServiceCollection services)
    {
        services.TryAddDevicePanel<K8055Panel>();
        services.TryAddDevicePanel<BusylightPanel>();
        services.TryAddDevicePanel<ZoomH4nPanel>();
        services.TryAddDevicePanel<RadexOnePanel>();
        services.TryAddDevicePanel<De5000Panel>();
        services.TryAddDevicePanel<LanCPanel>();
        services.AddTransient<IPresenter, ScpiReplyPresenter>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IInstrumentPanelProvider, ScpiInstrumentPanelProvider>());
        return services;
    }
}
