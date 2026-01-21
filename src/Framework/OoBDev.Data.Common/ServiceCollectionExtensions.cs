using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.Data.Common;

/// <summary>
/// Provides extension methods for registering data common services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds data common services to the service collection.
    /// Registers the generic IDatabaseQuery implementation.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection TryAddDataCommonServices(this IServiceCollection services)
    {
        services.AddTransient(typeof(IDatabaseQuery<>), typeof(DatabaseQuery<>));
        return services;
    }
}
