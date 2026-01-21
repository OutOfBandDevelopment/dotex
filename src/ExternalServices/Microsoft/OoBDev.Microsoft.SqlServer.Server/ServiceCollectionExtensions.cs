using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.Data.Common;

namespace OoBDev.Microsoft.SqlServer.Server;

/// <summary>
/// Provides extension methods for configuring Microsoft SQL Server services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers SQL Server database mapper services in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddMicrosoftSqlServerExtensions(this IServiceCollection services)
    {
        services.TryAddTransient<IDatabaseMapper, SqlDatabaseMapper>();
        services.TryAddKeyedTransient<IDatabaseMapper, SqlDatabaseMapper>("MSSQL");
        return services;
    }
}
