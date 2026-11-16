using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.Communications.Abstractions;
using System;

namespace OoBDev.Communications.EntityFrameworkCore;

/// <summary>Registers the database suppression list.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the suppression table and <see cref="EfSuppressionList"/> as the <see cref="ISuppressionList"/>. Call it before
    /// <c>AddEmailSuppression</c> so the in-memory list is not used.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Picks the database provider and connection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection TryAddEntityFrameworkSuppressionList(
        this IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        services.AddDbContextFactory<SuppressionsDbContext>(configure);
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<ISuppressionList, EfSuppressionList>();
        return services;
    }
}
