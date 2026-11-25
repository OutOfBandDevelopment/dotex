using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.System.IO.Bluetooth;

/// <summary>Registers the BLE device adapter.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="BleDeviceAdapter"/> as <see cref="IDeviceAdapter"/> with its options, plus a default "no adapter available"
    /// <see cref="IBleConnectionFactory"/>/<see cref="IBleDeviceDiscovery"/> pair. A per-OS backend
    /// (<c>OoBDev.System.IO.Bluetooth.Windows</c>) overrides these with a real implementation by
    /// registering its own after this call - the last registration wins when a service is resolved
    /// as a single instance, so callers must call this first. Callers still need to bind/configure
    /// <see cref="BleDeviceAdapterOptions"/> (device id, ...) themselves.
    /// </summary>
    public static IServiceCollection AddBleDeviceAdapter(this IServiceCollection services)
    {
        services.AddOptions<BleDeviceAdapterOptions>().ValidateDataAnnotations();
        services.TryAddSingleton<IBleConnectionFactory, UnsupportedPlatformBleConnectionFactory>();
        services.TryAddSingleton<IBleDeviceDiscovery, UnsupportedPlatformBleDeviceDiscovery>();
        services.TryAddSingleton<IBleGattProfileExplorer, UnsupportedPlatformBleGattProfileExplorer>();
        services.AddTransient<IDeviceAdapter, BleDeviceAdapter>();
        return services;
    }
}
