using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.AI;
using OoBDev.AllMiniLmL6V2Sharp;
using System;

namespace OoBDev.SBert.AllMiniLML6v2Sharp;

/// <summary>
/// Provides extension methods for registering AllMiniLM-L6-v2 sentence embedding services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds AllMiniLM-L6-v2 sentence embedding services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration containing embedding options.</param>
    /// <param name="allMiniLmL6V2EmbeddingOptionSection">The configuration section name for embedding options. Defaults to "AllMiniLmL6V2EmbeddingOptions" in Release builds.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection TryAddAllMiniLmL6V2Services(
        this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        string allMiniLmL6V2EmbeddingOptionSection
#else
        string allMiniLmL6V2EmbeddingOptionSection = nameof(AllMiniLmL6V2EmbeddingOptions)
#endif
        )
    {
        services.Configure<AllMiniLmL6V2EmbeddingOptions>(options => configuration.Bind(allMiniLmL6V2EmbeddingOptionSection, options));

        services.TryAddSingleton<IEmbedder, CachedAllMiniLmL6V2Embedder>();

        services.Replace(ServiceDescriptor.Transient(typeof(IEmbeddingProvider), typeof(AllMiniLmL6V2Embedding)));
        services.TryAddKeyedTransient<IEmbeddingProvider, AllMiniLmL6V2Embedding>("ALLMINILM");

        return services;
    }
}
