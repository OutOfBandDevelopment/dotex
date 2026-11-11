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
    /// Registers the context factory with the given provider setup and both stores under the key
    /// <see cref="EntityFrameworkEventSourcingGlobals.EntityFrameworkKey"/>. The provider packages call this with their <c>Use*</c> method.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Sets the database provider and connection on the context options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddEntityFrameworkEventSourcing(this IServiceCollection services, Action<IServiceProvider, DbContextOptionsBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        services.TryAddSingleton(TimeProvider.System);
        services.AddDbContextFactory<EventSourcingDbContext>(configure);
        services.TryAddKeyedSingleton<IEventStore>(
            EntityFrameworkEventSourcingGlobals.EntityFrameworkKey,
            (sp, _) => new EfEventStore(sp.GetRequiredService<IDbContextFactory<EventSourcingDbContext>>(), sp.GetRequiredService<TimeProvider>()));
        services.TryAddKeyedSingleton<IEntityStore>(
            EntityFrameworkEventSourcingGlobals.EntityFrameworkKey,
            (sp, _) => new EfEntityStore(sp.GetRequiredService<IDbContextFactory<EventSourcingDbContext>>()));
        return services;
    }
}
