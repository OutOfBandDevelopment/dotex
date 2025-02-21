using OoBDev.Search.Semantic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.Qdrant;

/// <summary>
/// Provides extension methods for configuring services related to Qdrant.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures services for Qdrant.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> to bind Qdrant options from.</param>
    /// <param name="qdrantOptionSection">The configuration section name containing Qdrant options.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddQdrantServices(
        this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        string qdrantOptionSection
#else
        string qdrantOptionSection = nameof(QdrantOptions)
#endif
        )
    {
        services.Configure<QdrantOptions>(options => configuration.Bind(qdrantOptionSection, options));
        services.TryAddTransient<IQdrantGrpcClientFactory, QdrantGrpcClientFactory>();
        services.TryAddTransient(sp => sp.GetRequiredService<IQdrantGrpcClientFactory>().Create());

        services.AddTransient<IVectorStoreProviderFactory, QdrantVectorStoreProviderFactory>();

        services.TryAddTransient<IPointStructFactory, PointStructFactory>();

        return services;
    }
}
