using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.System.IO.UsbHids;

/// <summary>Registers the options-driven USB HID device adapter.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="UsbHidEndpointDeviceAdapter"/> as <see cref="IDeviceAdapter"/> with its options and device services.
    /// Callers still bind or configure <see cref="UsbHidEndpointOptions"/> (vendor id, product id, ...).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same collection.</returns>
    public static IServiceCollection AddUsbHidEndpointDeviceAdapter(this IServiceCollection services)
    {
        services.AddOptions<UsbHidEndpointOptions>().ValidateDataAnnotations();
        services.AddSingleton<IUsbHidDeviceFactory, SystemUsbHidDeviceFactory>();
        services.AddSingleton<IUsbHidDiscovery, SystemUsbHidDiscovery>();
        services.AddTransient<IDeviceAdapter, UsbHidEndpointDeviceAdapter>();
        return services;
    }
}
