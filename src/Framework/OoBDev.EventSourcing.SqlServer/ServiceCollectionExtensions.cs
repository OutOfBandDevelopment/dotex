using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OoBDev.EventSourcing.EntityFrameworkCore;
using System;

namespace OoBDev.EventSourcing.SqlServer;

/// <summary>
/// Registers the SQL Server event and entity stores.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the SQL Server stores under <see cref="SqlServerGlobals.ProviderKey"/>. The schema is deployed from the
    /// <c>OoBDev.EventSourcing.SqlServer.Database</c> dacpac, not by EF migrations.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The SQL Server connection string.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddSqlServerEventSourcing(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        return services.TryAddEntityFrameworkEventSourcing(SqlServerGlobals.ProviderKey, (_, options) => options.UseSqlServer(connectionString));
    }
}
