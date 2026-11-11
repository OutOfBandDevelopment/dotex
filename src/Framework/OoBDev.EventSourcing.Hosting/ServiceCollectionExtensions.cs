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
}
