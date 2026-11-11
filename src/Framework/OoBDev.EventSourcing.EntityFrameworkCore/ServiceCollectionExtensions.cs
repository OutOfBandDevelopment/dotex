using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace OoBDev.EventSourcing.EntityFrameworkCore;

/// <summary>
/// Registers the Entity Framework Core stores.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the context factory with the given provider setup and both stores under <paramref name="providerKey"/>.
    /// The database packages call this with their key and <c>Use*</c> method. The context factory is registered once,
    /// so one container uses one database provider for the Entity Framework stores.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="providerKey">The kebab-case key to put in <c>OoBDev:EventSourcing:EventStore:Type</c> and <c>EntityStore:Type</c>.</param>
    /// <param name="configure">Sets the database provider and connection on the context options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddEntityFrameworkEventSourcing(this IServiceCollection services, string providerKey, Action<IServiceProvider, DbContextOptionsBuilder> configure)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerKey);
        ArgumentNullException.ThrowIfNull(configure);
        services.TryAddSingleton(TimeProvider.System);
        services.AddDbContextFactory<EventSourcingDbContext>(configure);
        services.TryAddKeyedSingleton<IEventStore>(
            providerKey,
            (sp, _) => new EfEventStore(sp.GetRequiredService<IDbContextFactory<EventSourcingDbContext>>(), sp.GetRequiredService<TimeProvider>()));
        services.TryAddKeyedSingleton<IEntityStore>(
            providerKey,
            (sp, _) => new EfEntityStore(sp.GetRequiredService<IDbContextFactory<EventSourcingDbContext>>()));
        return services;
    }
}
