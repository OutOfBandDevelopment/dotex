using OoBDev.Search.Semantic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OoBDev.Search.Fusion;
using OoBDev.System;

namespace OoBDev.Search;

/// <summary>
/// Provides extension methods for configuring search-related services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures services for search functionality.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddSearchServices(this IServiceCollection services)
    {
        services.TryAddTransient<IVectorStoreFactory, VectorStoreFactory>();
        services.TryAddTransient(typeof(IVectorStore<>), typeof(WrappedVectorStore<>));

        return services;
    }

    /// <summary>
    /// Registers the <see cref="IScoreFusion"/> chosen by <see cref="SearchFusionOptions.Strategy"/> (<see cref="SearchFusionOptions.SectionName"/>).
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddSearchFusion(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<SearchFusionOptions>(configuration, SearchFusionOptions.SectionName);
        services.TryAddSingleton<IScoreFusion>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<SearchFusionOptions>>().Value;
            return options.Strategy == FusionNames.ReciprocalRank ? new ReciprocalRankFusion(options.K) : new MaxNormalisedFusion();
        });

        return services;
    }
}
