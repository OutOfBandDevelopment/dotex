using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Neo4j.Driver;
using OoBDev.Search.Graph;

namespace OoBDev.Neo4j;

/// <summary>
/// Registers the Neo4j graph store.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Binds <see cref="Neo4jOptions"/>, registers the driver as a singleton and <see cref="IGraphStore"/> backed by Neo4j.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="optionSection">The configuration section.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddNeo4jServices(
        this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        string optionSection
#else
        string optionSection = nameof(Neo4jOptions)
#endif
        )
    {
        services.AddOptions<Neo4jOptions>()
            .Bind(configuration.GetSection(optionSection))
            .ValidateDataAnnotations();
        services.TryAddSingleton(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<Neo4jOptions>>().Value;
            return GraphDatabase.Driver(settings.Uri, AuthTokens.Basic(settings.User, settings.Password));
        });
        services.TryAddTransient<IGraphStore, Neo4jGraphStore>();
        return services;
    }
}
