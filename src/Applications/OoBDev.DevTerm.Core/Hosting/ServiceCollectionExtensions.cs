using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.DevTerm.Hosting;

/// <summary>Registers the DevTerm core engine.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the presenter catalog and session factory every front end composes with its own transports and presenters.
    /// The catalog is transient because presenters are stateful: each session resolves one catalog and keeps it, so
    /// sessions never share (or interleave partial frames through) the same presenter instances.
    /// </summary>
    public static IServiceCollection AddDevTermCore(this IServiceCollection services)
    {
        services.TryAddTransient<PresenterCatalog>();
        services.TryAddSingleton<ISessionFactory, SessionFactory>();
        return services;
    }
}
