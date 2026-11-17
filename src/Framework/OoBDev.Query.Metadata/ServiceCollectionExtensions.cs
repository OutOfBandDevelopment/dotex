using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.System;

namespace OoBDev.Query.Metadata;

/// <summary>
/// Registers the metadata query engine.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Binds <see cref="QueryMetadataOptions"/> from <see cref="QueryMetadataOptions.SectionName"/>, scans the assemblies for kinds and registers the engine.
    /// A database adapter must register <see cref="IQueryDialect"/> and <see cref="IQueryExecutor"/>; a host may register <see cref="IQueryAuthorizer"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="kindAssemblies">The assemblies holding the row classes marked with <see cref="QueryKindAttribute"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddMetadataQuery(this IServiceCollection services, IConfiguration configuration, params Assembly[] kindAssemblies)
    {
        ArgumentNullException.ThrowIfNull(kindAssemblies);
        services.AddValidatedOptions<QueryMetadataOptions>(configuration, QueryMetadataOptions.SectionName);
        services.TryAddSingleton(new KindRegistry(kindAssemblies));
        services.TryAddSingleton<CursorCodec>();
        services.TryAddSingleton<QueryEngine>();
        return services;
    }
}
