using AllMiniLmL6V2Sharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.AI;
using System;

namespace OoBDev.SBert.AllMiniLML6v2Sharp;

public static class ServiceCollectionExtensions
{
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
