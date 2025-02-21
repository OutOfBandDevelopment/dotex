using OoBDev.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.Ollama;

/// <summary>
/// Provides extension methods for configuring services related to Ollama.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures services for Ollama.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The configuration containing the Ollama API client options.</param>
    /// <param name="ollamaApiClientOptionSection">The name of the configuration section containing the Ollama API client options.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddOllamaServices(
        this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        string ollamaApiClientOptionSection
#else
        string ollamaApiClientOptionSection = nameof(OllamaApiClientOptions)
#endif
        )
    {
        var url = configuration.GetSection(ollamaApiClientOptionSection)?[nameof(OllamaApiClientOptions.Url)];
        if (url == null)
        {
            return services;
        }
        services.AddHealthChecks().AddCheck<OllamaHealthCheck>("ollama");

        services.TryAddTransient<IOllamaApiClientFactory, OllamaApiClientFactory>();
        services.TryAddTransient<IOllamaModelMapper, OllamaModelMapper>();
        services.TryAddTransient(sp => ActivatorUtilities.CreateInstance<OllamaMessageCompletion>(sp));
        services.TryAddTransient(sp => sp.GetRequiredService<IOllamaApiClientFactory>().Build());

        services.TryAddTransient<IMessageCompletion, OllamaMessageCompletion>();
        //services.TryAddTransient<ILanguageModelProvider, OllamaMessageCompletion>(); //TODO: restore support? 
        services.TryAddTransient<IEmbeddingProvider, OllamaMessageCompletion>();

        services.TryAddKeyedTransient<IMessageCompletion, OllamaMessageCompletion>("OLLAMA");
        //services.TryAddKeyedTransient<ILanguageModelProvider, OllamaMessageCompletion>("OLLAMA");
        services.TryAddKeyedTransient<IEmbeddingProvider, OllamaMessageCompletion>("OLLAMA");

        services.Configure<OllamaApiClientOptions>(options => configuration.Bind(ollamaApiClientOptionSection, options));

        return services;
    }
}
