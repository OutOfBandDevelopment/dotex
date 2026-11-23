using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.ComplexEvents.EntityFrameworkCore;

/// <summary>Registers the database schedule store.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds the schedule and last-run tables, <see cref="EfScheduleStore"/> as the <see cref="IScheduleStore"/> and <see cref="EfLastRunStore"/> as the <see cref="ILastRunStore"/>.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Picks the database provider and connection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection TryAddEntityFrameworkScheduleStore(this IServiceCollection services, Action<IServiceProvider, DbContextOptionsBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        services.AddDbContextFactory<ScheduleDbContext>(configure);
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<ILastRunStore>(sp => new EfLastRunStore(sp.GetRequiredService<IDbContextFactory<ScheduleDbContext>>()));
        services.TryAddSingleton<IScheduleStore>(sp => new EfScheduleStore(sp.GetRequiredService<IDbContextFactory<ScheduleDbContext>>(), sp.GetRequiredService<TimeProvider>()));
        return services;
    }
}
