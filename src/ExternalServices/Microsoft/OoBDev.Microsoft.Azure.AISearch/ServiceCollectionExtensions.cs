using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.Search.Semantic;

namespace OoBDev.Microsoft.Azure.AISearch;

/// <summary>
/// Registers the Azure AI Search adapter.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Binds <see cref="AzureAISearchOptions"/> and registers the vector store provider factory.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="optionSection">The configuration section.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddAzureAISearchServices(
        this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        string optionSection
#else
        string optionSection = nameof(AzureAISearchOptions)
#endif
        )
    {
        services.AddOptions<AzureAISearchOptions>()
            .Bind(configuration.GetSection(optionSection))
            .ValidateDataAnnotations();
        services.TryAddTransient<IVectorStoreProviderFactory, AzureAISearchVectorStoreProviderFactory>();
        services.AddTransient<IVectorStoreCollectionCatalog>(sp => new AzureAISearchVectorStoreCollectionCatalog(sp.GetRequiredService<global::Microsoft.Extensions.Options.IOptions<AzureAISearchOptions>>()));
        return services;
    }
}
