using OoBDev.System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace OoBDev.DevTerm.Transports.Loopback;

/// <summary>Registers the loopback transport.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers <see cref="LoopbackTransport"/> as a transient <see cref="IDeviceAdapter"/> with validated options.</summary>
    public static IServiceCollection TryAddLoopbackTransport(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<LoopbackTransportOptions>, LoopbackTransportOptionsValidator>());
        services.AddOptions<LoopbackTransportOptions>().ValidateOnStart();
        services.TryAddTransient<IDeviceAdapter, LoopbackTransport>();
        return services;
    }
}
