using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OoBDev.EventSourcing.EntityFrameworkCore;
using System;

namespace OoBDev.EventSourcing.Sqlite;

/// <summary>
/// Registers the SQLite event and entity stores.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the SQLite stores under <see cref="SqliteGlobals.ProviderKey"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The SQLite connection string.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddSqliteEventSourcing(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        return services.TryAddEntityFrameworkEventSourcing(
            SqliteGlobals.ProviderKey,
            (_, options) => options.UseSqlite(connectionString));
    }
}
