using OoBDev.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;

namespace OoBDev.SBert;

/// <summary>
/// Provides extension methods for configuring services related to SBERT (Sentence-BERT).
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures services for SBERT (Sentence-BERT).
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> to bind SBERT options from.</param>
    /// <param name="sentenceEmbeddingOptionSection">The configuration section name containing SBERT options.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddSbertServices(
        this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        string sentenceEmbeddingOptionSection
#else
        string sentenceEmbeddingOptionSection = nameof(SentenceEmbeddingOptions)
#endif
        )
    {
        var url = configuration.GetSection(sentenceEmbeddingOptionSection)?[nameof(SentenceEmbeddingOptions.Url)];
        if (url == null)
        {
            return services;
        }
        services.AddHealthChecks().AddCheck<SbertHealthCheck>("sbert");

        services.Configure<SentenceEmbeddingOptions>(options => configuration.Bind(sentenceEmbeddingOptionSection, options));
        services.TryAddTransient<IEmbeddingProvider, SentenceEmbeddingProvider>();
        services.TryAddKeyedTransient<IEmbeddingProvider, SentenceEmbeddingProvider>("SBERT");

        services.AddHttpClient<ISentenceEmbeddingClient, SentenceEmbeddingClient>((sp, http) =>
        {
            var options = sp.GetRequiredService<IOptions<SentenceEmbeddingOptions>>();
            http.BaseAddress = new Uri(options.Value.Url);
        });

        return services;
    }
}
