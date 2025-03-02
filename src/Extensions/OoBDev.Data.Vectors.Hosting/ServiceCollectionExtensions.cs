using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.Data.Vectors.Hosting;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection TryAddVectorHosting(this IServiceCollection services)
    {
        services.TryAddTransient<IEmbeddingSentenceTransformerQueueReader, EmbeddingSentenceTransformerQueueReader>();
        services.AddHostedService<EmbeddingSentenceTransformerQueueReaderHost>();
        return services;
    }
}
