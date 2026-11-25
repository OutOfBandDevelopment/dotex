using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace OoBDev.System.IO.Tcp;

/// <summary>Registers the TCP-based device adapters.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the raw TCP/Telnet adapter and its options. Callers still bind <see cref="TcpEndpointOptions"/> (mode, host, port).
    /// </summary>
    public static IServiceCollection AddTcpEndpointDeviceAdapter(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<TcpEndpointOptions>, TcpEndpointOptionsValidator>());
        services.AddOptions<TcpEndpointOptions>().ValidateOnStart();
        services.TryAddSingleton<ITcpConnectionSource, SystemTcpConnectionSource>();
        services.AddTransient<IDeviceAdapter, TcpEndpointDeviceAdapter>();
        return services;
    }

    /// <summary>
    /// Registers the RFC 2217 client adapter and its options. Callers still bind <see cref="Rfc2217ClientOptions"/>.
    /// </summary>
    public static IServiceCollection AddRfc2217ClientDeviceAdapter(this IServiceCollection services)
    {
        services.AddOptions<Rfc2217ClientOptions>().ValidateDataAnnotations().ValidateOnStart();
        services.TryAddSingleton<ITcpConnectionSource, SystemTcpConnectionSource>();
        services.AddTransient<IDeviceAdapter, Rfc2217ClientDeviceAdapter>();
        return services;
    }

    /// <summary>
    /// Registers the VXI-11 adapter and its options. Callers still bind <see cref="Vxi11DeviceAdapterOptions"/>.
    /// </summary>
    public static IServiceCollection AddVxi11DeviceAdapter(this IServiceCollection services)
    {
        services.AddOptions<Vxi11DeviceAdapterOptions>().ValidateDataAnnotations().ValidateOnStart();
        services.TryAddSingleton<ITcpConnectionSource, SystemTcpConnectionSource>();
        services.AddTransient<IDeviceAdapter, Vxi11DeviceAdapter>();
        return services;
    }
}
