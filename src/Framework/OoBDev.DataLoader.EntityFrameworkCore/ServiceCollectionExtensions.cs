using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.DataLoader.EntityFrameworkCore;

/// <summary>Registers the database row sink.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds the rows table and <see cref="EfRowSink"/> as the <see cref="IRowSink"/>.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="dataset">The dataset name stored with each row.</param>
    /// <param name="keyColumn">The column that identifies a row.</param>
    /// <param name="configure">Picks the database provider and connection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection TryAddEntityFrameworkRowSink(this IServiceCollection services, string dataset, string keyColumn, Action<IServiceProvider, DbContextOptionsBuilder> configure)
    {
        ArgumentException.ThrowIfNullOrEmpty(dataset);
        ArgumentException.ThrowIfNullOrEmpty(keyColumn);
        ArgumentNullException.ThrowIfNull(configure);
        services.AddDbContextFactory<LoadedRowsDbContext>(configure);
        services.TryAddSingleton<IRowSink>(sp => new EfRowSink(sp.GetRequiredService<IDbContextFactory<LoadedRowsDbContext>>(), dataset, keyColumn));
        return services;
    }
}
