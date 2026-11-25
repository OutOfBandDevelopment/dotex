using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace OoBDev.EventSourcing.Hosting;

/// <summary>
/// Registers the hosted event relay.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="EventRelayHost"/> as a hosted service. Run one host per relay name (<see cref="EventRelayOptions.Name"/>).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddEventRelayHost(this IServiceCollection services)
    {
        services.TryAddEventSourcing();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, EventRelayHost>());
        return services;
    }

    /// <summary>
    /// Adds <see cref="ErasureCompactionHost"/> as a hosted service so recorded erasure requests are completed on a timer
    /// (<c>OoBDev:EventSourcing:Erasure:PollInterval</c>, default one minute).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddErasureCompactionHost(this IServiceCollection services)
    {
        services.TryAddEventSourcing();
        services.AddOptions<ErasureJobOptions>().BindConfiguration(EventSourcingGlobals.ErasureSection).ValidateDataAnnotations();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, ErasureCompactionHost>());
        return services;
    }
}
