using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.OpenSearch;

/// <summary>
/// Provides extension methods for configuring services related to OpenSearch.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures services for OpenSearch.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> to bind OpenSearch options from.</param>
    /// <param name="openSearchOptionSections">The configuration section name containing OpenSearch options.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddOpenSearchServices(
        this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        string openSearchOptionSections
#else
        string openSearchOptionSections = nameof(OpenSearchOptions)
#endif
        )
    {
        services.Configure<OpenSearchOptions>(options => configuration.Bind(openSearchOptionSections, options));
        services.TryAddTransient<IOpenSearchClientFactory, OpenSearchClientFactory>();
        services.TryAddTransient(sp => sp.GetRequiredService<IOpenSearchClientFactory>().Create());

        return services;
    }
}
