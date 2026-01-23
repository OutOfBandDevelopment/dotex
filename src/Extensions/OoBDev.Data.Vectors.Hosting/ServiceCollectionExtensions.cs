using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.Data.Vectors.Hosting;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register vector hosting services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers embedding sentence transformer queue reader services and hosted service.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration to bind options from.</param>
    /// <param name="configurationSectionName">The configuration section name for options. Defaults to the options class name.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddVectorHosting(this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        string configurationSectionName
#else
        string configurationSectionName = nameof(EmbeddingSentenceTransformerQueueReaderOptions)
#endif
        )
    {
        services.Configure<EmbeddingSentenceTransformerQueueReaderOptions>(options => configuration.Bind(configurationSectionName, options));

        services.TryAddTransient<IEmbeddingSentenceTransformerQueueReader, EmbeddingSentenceTransformerQueueReader>();
        services.AddHostedService<EmbeddingSentenceTransformerQueueReaderHost>();
        return services;
    }
}
