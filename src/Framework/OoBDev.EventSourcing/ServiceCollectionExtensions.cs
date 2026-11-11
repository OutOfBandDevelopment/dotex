using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OoBDev.MessageQueueing;
using OoBDev.System;
using System;

namespace OoBDev.EventSourcing;

/// <summary>
/// Registers the event sourcing services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the in-memory stores as defaults, the configured selection of the event store
    /// (<see cref="EventSourcingGlobals.EventStorePath"/>) and entity store (<see cref="EventSourcingGlobals.EntityStorePath"/>),
    /// and the <see cref="EventRelay"/>. Other providers register their store under a kebab-case key.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddEventSourcing(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IEventStore, InMemoryEventStore>();
        services.TryAddSingleton<IEntityStore, InMemoryEntityStore>();
        services.TryAddKeyedSingleton<IEventStore>(EventSourcingGlobals.InMemoryKey, (sp, _) => sp.GetRequiredService<IEventStore>());
        services.TryAddKeyedSingleton<IEntityStore>(EventSourcingGlobals.InMemoryKey, (sp, _) => sp.GetRequiredService<IEntityStore>());
        services.TryAddConfiguredKeyedService<IEventStore>(EventSourcingGlobals.EventStorePath, EventSourcingGlobals.EventStoreSelectedKey);
        services.TryAddConfiguredKeyedService<IEntityStore>(EventSourcingGlobals.EntityStorePath, EventSourcingGlobals.EntityStoreSelectedKey);
        services.AddOptions<EventRelayOptions>().BindConfiguration(EventSourcingGlobals.RelaySection).ValidateDataAnnotations();
        services.TryAddTransient(sp => new EventRelay(
            sp.GetRequiredKeyedService<IEventStore>(EventSourcingGlobals.EventStoreSelectedKey),
            sp.GetRequiredKeyedService<IEntityStore>(EventSourcingGlobals.EntityStoreSelectedKey),
            sp.GetRequiredService<IMessageQueueSender<EventsChannel>>(),
            sp.GetRequiredService<IOptions<EventRelayOptions>>(),
            sp.GetRequiredService<TimeProvider>()));
        services.TryAddSingleton<EventTypeRegistry>();
        services.TryAddSingleton<EventUpcastPipeline>();
        services.TryAddTransient(sp => new ProjectionRunner(
            sp.GetRequiredKeyedService<IEventStore>(EventSourcingGlobals.EventStoreSelectedKey),
            sp.GetRequiredKeyedService<IEntityStore>(EventSourcingGlobals.EntityStoreSelectedKey),
            sp.GetServices<IProjection>()));
        services.TryAddTransient(typeof(IAggregateRepository<>), typeof(AggregateRepository<>));
        return services;
    }
}
