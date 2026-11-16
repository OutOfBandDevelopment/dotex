using System;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OoBDev.AI;
using OoBDev.System;

namespace OoBDev.Embeddings.Http;

/// <summary>
/// Registers the HTTP embedding client.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="OpenAiCompatibleEmbeddingProvider"/> as <see cref="IEmbeddingProvider"/> (and under <see cref="EmbeddingsHttpGlobals.ProviderKey"/>), binding <see cref="HttpEmbeddingOptions"/> from <see cref="HttpEmbeddingOptions.SectionName"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddHttpEmbeddings(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<HttpEmbeddingOptions>(configuration, HttpEmbeddingOptions.SectionName);
        services.AddHttpClient<OpenAiCompatibleEmbeddingProvider>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<HttpEmbeddingOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            if (!string.IsNullOrEmpty(options.ApiKey))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
        });
        services.TryAddTransient<IEmbeddingProvider>(sp => sp.GetRequiredService<OpenAiCompatibleEmbeddingProvider>());
        services.TryAddKeyedTransient<IEmbeddingProvider>(EmbeddingsHttpGlobals.ProviderKey, (sp, _) => sp.GetRequiredService<OpenAiCompatibleEmbeddingProvider>());
        return services;
    }
}
