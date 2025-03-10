using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.Data.Vectors.Hosting;

public static class ServiceCollectionExtensions
{
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
