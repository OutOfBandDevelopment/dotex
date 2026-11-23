using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.ComplexEvents;

namespace OoBDev.Microsoft.Azure.EventHub;

/// <summary>Registers the Azure Event Hubs client.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds <see cref="AzureEventHubClient"/> as the <see cref="IEventHubClient"/>.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Sets the connection string and hub name when not bound from configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection TryAddAzureEventHub(this IServiceCollection services, Action<AzureEventHubOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddOptions<AzureEventHubOptions>();
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.TryAddSingleton<AzureEventHubClient>();
        services.TryAddSingleton<IEventHubClient>(sp => sp.GetRequiredService<AzureEventHubClient>());
        return services;
    }
}
