using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.Search;
using OoBDev.System;

namespace OoBDev.Data.Vectors.Postgres;

/// <summary>
/// Registers the pgvector store.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="PgVectorStore"/> as <see cref="IVectorStore"/> (and under <see cref="PgVectorGlobals.ProviderKey"/>), binding <see cref="PgVectorOptions"/> from <see cref="PgVectorOptions.SectionName"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddPgVectorStore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<PgVectorOptions>(configuration, PgVectorOptions.SectionName);
        services.TryAddSingleton<PgVectorStore>();
        services.TryAddSingleton<IVectorStore>(sp => sp.GetRequiredService<PgVectorStore>());
        services.TryAddKeyedSingleton<IVectorStore>(PgVectorGlobals.ProviderKey, (sp, _) => sp.GetRequiredService<PgVectorStore>());
        return services;
    }
}
