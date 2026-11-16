using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.Query.Metadata;
using OoBDev.Search;
using OoBDev.System;

namespace OoBDev.PostgreSql.Query;

/// <summary>
/// Registers the metadata query engine on PostgreSQL.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the engine from <see cref="OoBDev.Query.Metadata.ServiceCollectionExtensions.TryAddMetadataQuery"/> with
    /// <see cref="PostgresQueryDialect"/> and <see cref="NpgsqlQueryExecutor"/>, and binds <see cref="PostgresQueryOptions"/> from <see cref="PostgresQueryOptions.SectionName"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="kindAssemblies">The assemblies holding the row classes marked with <see cref="QueryKindAttribute"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddPostgresMetadataQuery(this IServiceCollection services, IConfiguration configuration, params Assembly[] kindAssemblies)
    {
        services.TryAddMetadataQuery(configuration, kindAssemblies);
        services.AddValidatedOptions<PostgresQueryOptions>(configuration, PostgresQueryOptions.SectionName);
        services.TryAddSingleton<IQueryDialect, PostgresQueryDialect>();
        services.TryAddSingleton<IQueryExecutor, NpgsqlQueryExecutor>();
        return services;
    }

    /// <summary>The key of the SQL-kind provider among the keyed <see cref="ISearchProvider"/> registrations.</summary>
    public const string SqlKindProviderKey = "postgres-sql-kind";

    /// <summary>Registers <see cref="SqlKindSearchProvider"/> (keyed <see cref="SqlKindProviderKey"/>); needs <see cref="TryAddPostgresMetadataQuery"/>.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration holding <see cref="SqlKindSearchOptions.SectionName"/>.</param>
    public static IServiceCollection TryAddPostgresSqlKindSearch(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<SqlKindSearchOptions>(configuration, SqlKindSearchOptions.SectionName);
        services.TryAddSingleton<SqlKindSearchProvider>();
        services.TryAddKeyedSingleton<ISearchProvider>(SqlKindProviderKey, (sp, _) => sp.GetRequiredService<SqlKindSearchProvider>());
        return services;
    }
}
