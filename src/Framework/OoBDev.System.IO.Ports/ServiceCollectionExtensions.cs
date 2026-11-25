using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.System.IO.Ports;

/// <summary>Registers the options-driven serial device adapter.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="SerialDeviceAdapter"/> as <see cref="IDeviceAdapter"/> with its options and port services.
    /// Callers still bind or configure <see cref="SerialDeviceAdapterOptions"/> (port name, baud rate, ...).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same collection.</returns>
    public static IServiceCollection AddSerialDeviceAdapter(this IServiceCollection services)
    {
        services.AddOptions<SerialDeviceAdapterOptions>().ValidateDataAnnotations();
        services.AddSingleton<ISerialPortFactory, SystemSerialPortFactory>();
        services.AddSingleton<ISerialPortDiscovery, SystemSerialPortDiscovery>();
        services.AddTransient<IDeviceAdapter, SerialDeviceAdapter>();
        return services;
    }
}
