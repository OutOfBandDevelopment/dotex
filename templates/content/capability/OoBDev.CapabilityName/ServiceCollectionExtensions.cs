using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.System;

namespace OoBDev.CapabilityName;

/// <summary>
/// Registration for CapabilityName.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers CapabilityName services using TryAdd semantics.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="builder">Optional overrides.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddCapabilityNameServices(
        this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        CapabilityNameBuilder? builder
#else
        CapabilityNameBuilder? builder = default
#endif
        )
    {
        builder ??= new CapabilityNameBuilder();

        services.TryAddProviders(); // IStringFormatter and ISelectedService<T>
        services.Configure<CapabilityNameOptions>(o => configuration.Bind(builder.OptionsSection, o));
        services.TryAddTransient<ICapabilityNameManager, CapabilityNameManager>();
        return services;
    }
}
