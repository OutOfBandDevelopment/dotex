using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.System.IO.Bluetooth.Windows;

/// <summary>Registers the Windows BLE backend.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// The entry point an application looks up by name via reflection (this assembly targets a
    /// Windows TFM, so a plain net10.0 application cannot reference it) and invokes after <c>OoBDev.System.IO.Bluetooth.ServiceCollectionExtensions.AddBleDeviceAdapter</c>,
    /// overriding its default "unsupported platform" registrations with these real
    /// Windows.Devices.Bluetooth-backed ones. Never called directly from a project reference - see
    /// this assembly's csproj comment for why.
    /// </summary>
    public static IServiceCollection AddWindowsBleConnection(this IServiceCollection services)
    {
        services.AddSingleton<IBleConnectionFactory, WindowsBleConnectionFactory>();
        services.AddSingleton<IBleDeviceDiscovery, WindowsBleDeviceDiscovery>();
        services.AddSingleton<IBleGattProfileExplorer, WindowsBleGattProfileExplorer>();
        return services;
    }
}
