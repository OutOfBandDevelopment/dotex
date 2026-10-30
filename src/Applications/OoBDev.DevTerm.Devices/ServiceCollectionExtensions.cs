using Microsoft.Extensions.DependencyInjection;
using OoBDev.DevTerm.Panels;
using OoBDev.Kuando.Busylight.Panels;
using OoBDev.Velleman.K8055.Panels;

namespace OoBDev.DevTerm.Devices;

/// <summary>Registers the device panels DevTerm ships with.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds the K8055 and Busylight panels.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same collection.</returns>
    public static IServiceCollection AddDevTermDevicePanels(this IServiceCollection services)
    {
        services.TryAddDevicePanel<K8055Panel>();
        services.TryAddDevicePanel<BusylightPanel>();
        return services;
    }
}
