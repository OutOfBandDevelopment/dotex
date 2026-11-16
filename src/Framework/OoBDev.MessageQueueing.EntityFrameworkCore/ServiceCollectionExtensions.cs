using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.MessageQueueing.Services;
using System;

namespace OoBDev.MessageQueueing.EntityFrameworkCore;

/// <summary>Registers the relational inbox and outbox.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the context factory and the relational <see cref="IInboxStore"/> and <see cref="IOutboxStore"/>. Call it
    /// before <c>TryAddMessageQueueHosting</c>, whose in-memory outbox is a <c>TryAdd</c> and would otherwise win.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Sets the database provider and connection on the context options.</param>
    /// <param name="messageTypes">Adds the message types the outbox may store.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddEntityFrameworkMessaging(
        this IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder> configure,
        Action<OutboxMessageTypes>? messageTypes = null)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var types = new OutboxMessageTypes();
        messageTypes?.Invoke(types);
        services.TryAddSingleton(types);
        services.AddDbContextFactory<MessagingDbContext>(configure);
        services.TryAddSingleton<IInboxStore>(sp => new EfInboxStore(sp.GetRequiredService<IDbContextFactory<MessagingDbContext>>()));
        services.TryAddSingleton<IOutboxStore>(sp => new EfOutboxStore(
            sp.GetRequiredService<IDbContextFactory<MessagingDbContext>>(),
            sp.GetRequiredService<OutboxMessageTypes>()));
        return services;
    }
}
