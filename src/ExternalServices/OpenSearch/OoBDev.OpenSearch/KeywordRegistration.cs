using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.Search;

namespace OoBDev.OpenSearch;

/// <summary>Registers the keyword search provider.</summary>
public static class KeywordRegistration
{
    /// <summary>The kebab-case key the provider is registered under as <see cref="ISearchProvider"/>.</summary>
    public const string ProviderKey = "opensearch-keyword";

    /// <summary>
    /// Registers <see cref="KeywordSearchProvider"/> (default and keyed <see cref="ProviderKey"/>), binding <see cref="KeywordSearchOptions"/> from <see cref="KeywordSearchOptions.SectionName"/>.
    /// The OpenSearch connection itself comes from <c>TryAddOpenSearchServices</c>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddOpenSearchKeywordSearch(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<KeywordSearchOptions>()
            .Bind(configuration.GetSection(KeywordSearchOptions.SectionName))
            .ValidateDataAnnotations();
        services.TryAddSingleton<KeywordSearchProvider>();
        services.TryAddKeyedSingleton<ISearchProvider>(ProviderKey, (sp, _) => sp.GetRequiredService<KeywordSearchProvider>());
        return services;
    }
}
