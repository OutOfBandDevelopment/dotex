using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OoBDev.AI;
using OoBDev.Onnx.SentenceEmbeddings;

namespace OoBDev.SBert.NomicEmbedTextV1_5;

/// <summary>
/// Registration of the in-process nomic-embed-text-v1.5 sentence embedding services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the keyed nomic-embed-text-v1.5 generator and <see cref="IEmbeddingProvider"/> (key <see cref="SBertGlobals.NomicEmbedTextV1_5Key"/>).
    /// The first model added also becomes the default unkeyed <see cref="IEmbeddingProvider"/> and generator, so several models can be registered side by side.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration containing <see cref="OnnxSentenceEmbeddingOptions"/>.</param>
    /// <param name="optionSection">The configuration section name for the options.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection TryAddNomicEmbedTextV1_5Services(
        this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        string optionSection
#else
        string optionSection = SBertGlobals.DefaultSection
#endif
        )
    {
        services.TryAddKeyedSingleton(SBertGlobals.NomicEmbedTextV1_5Key, (sp, _) =>
        {
            var options = new OnnxSentenceEmbeddingOptions();
            NomicEmbedTextV1_5Model.ApplyDefaults(options);
            configuration.Bind(optionSection, options);
            return new OnnxSentenceEmbeddingGenerator(Options.Create(options), sp.GetRequiredService<ILogger<OnnxSentenceEmbeddingGenerator>>());
        });
        services.TryAddKeyedSingleton<IEmbeddingGenerator<string, Embedding<float>>>(
            SBertGlobals.NomicEmbedTextV1_5Key, (sp, key) => sp.GetRequiredKeyedService<OnnxSentenceEmbeddingGenerator>(key));
        services.TryAddKeyedTransient<IEmbeddingProvider>(
            SBertGlobals.NomicEmbedTextV1_5Key, (sp, key) => new NomicEmbedTextV1_5EmbeddingProvider(sp.GetRequiredKeyedService<OnnxSentenceEmbeddingGenerator>(key)));

        services.TryAddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(
            sp => sp.GetRequiredKeyedService<IEmbeddingGenerator<string, Embedding<float>>>(SBertGlobals.NomicEmbedTextV1_5Key));
        services.TryAddTransient<IEmbeddingProvider>(
            sp => sp.GetRequiredKeyedService<IEmbeddingProvider>(SBertGlobals.NomicEmbedTextV1_5Key));

        return services;
    }
}
