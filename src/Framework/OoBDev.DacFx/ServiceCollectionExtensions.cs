// Ignore Spelling: Dac

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.DacFx;

/// <summary>
/// Provides extension methods for configuring DacPac services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers DacPac builder and validator services in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddDacPacServices(this IServiceCollection services)
    {
        services.TryAddTransient<IDacPacBuilder, DacPacBuilder>();
        services.TryAddTransient<IDacPacValidator, DacPacValidator>();
        return services;
    }
}
