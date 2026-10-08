using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.AI;
using OoBDev.Onnx.SentenceEmbeddings;
using System;

namespace OoBDev.SBert.AllMiniLmL6V2;

/// <summary>
/// Registration of the in-process all-MiniLM-L6-v2 sentence embedding services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the all-MiniLM-L6-v2 generator, <see cref="IEmbeddingProvider"/> and keyed providers.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration containing <see cref="OnnxSentenceEmbeddingOptions"/>.</param>
    /// <param name="allMiniLmL6V2OptionSection">The configuration section name for the options.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection TryAddAllMiniLmL6V2Services(
        this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        string allMiniLmL6V2OptionSection
#else
        string allMiniLmL6V2OptionSection = SBertGlobals.DefaultSection
#endif
        )
    {
        services.Configure<OnnxSentenceEmbeddingOptions>(options =>
        {
            AllMiniLmL6V2Model.ApplyDefaults(options);
            configuration.Bind(allMiniLmL6V2OptionSection, options);
        });

        services.TryAddSingleton<OnnxSentenceEmbeddingGenerator>();
        services.TryAddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp => sp.GetRequiredService<OnnxSentenceEmbeddingGenerator>());

        services.Replace(ServiceDescriptor.Transient<IEmbeddingProvider, AllMiniLmL6V2EmbeddingProvider>());
        services.TryAddKeyedTransient<IEmbeddingProvider, AllMiniLmL6V2EmbeddingProvider>(SBertGlobals.AllMiniLmL6V2Key);
        services.TryAddKeyedTransient<IEmbeddingProvider, AllMiniLmL6V2EmbeddingProvider>(SBertGlobals.LegacyKey);

        return services;
    }
}
