using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OoBDev.System.IO.Pipelines;

namespace OoBDev.System.IO.UsbTmc;

/// <summary>Registers the USBTMC device adapter.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="UsbTmcDeviceAdapter"/> as <see cref="IDeviceAdapter"/> with its options and device services. Callers still need to bind/configure
    /// <see cref="UsbTmcDeviceAdapterOptions"/> (vendor/product ID, ...) themselves.
    /// </summary>
    public static IServiceCollection AddUsbTmcDeviceAdapter(this IServiceCollection services)
    {
        services.AddOptions<UsbTmcDeviceAdapterOptions>().ValidateDataAnnotations();
        services.AddSingleton<IUsbTmcDeviceFactory, SystemUsbTmcDeviceFactory>();
        services.AddSingleton<IUsbTmcDeviceDiscovery, SystemUsbTmcDeviceDiscovery>();
        services.AddTransient<IDeviceAdapter, UsbTmcDeviceAdapter>();
        return services;
    }
}
