using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OoBDev.EventSourcing.EntityFrameworkCore;
using System;

namespace OoBDev.EventSourcing.Postgres;

/// <summary>
/// Registers the PostgreSQL event and entity stores.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the PostgreSQL stores under <see cref="PostgresGlobals.ProviderKey"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddPostgresEventSourcing(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        return services.TryAddEntityFrameworkEventSourcing(
            PostgresGlobals.ProviderKey,
            (_, options) => options.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(PostgresGlobals).Assembly.GetName().Name)));
    }
}
