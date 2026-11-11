using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing;

/// <summary>
/// Event-store backed repository: the stream id is <c>{AggregateType}-{id}</c> and events are stored as JSON.
/// </summary>
/// <typeparam name="T">The aggregate type.</typeparam>
/// <param name="store">The event store.</param>
/// <param name="types">The event type registry.</param>
public class AggregateRepository<T>([FromKeyedServices(EventSourcingGlobals.EventStoreSelectedKey)] IEventStore store, EventTypeRegistry types, EventUpcastPipeline? upcasts = null) : IAggregateRepository<T> where T : AggregateRoot, new()
{
    /// <summary>
    /// Gets the stream id of an aggregate.
    /// </summary>
    /// <param name="id">The aggregate identifier.</param>
    /// <returns>The stream id.</returns>
    public static string StreamIdOf(string id) => $"{typeof(T).Name}-{id}";

    /// <inheritdoc />
    public async Task<T?> LoadAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        var records = await store.ReadAsync(StreamIdOf(id), 1, cancellationToken).ConfigureAwait(false);
        if (records.Count == 0)
            return null;

        var history = records.Select(r => Upcast(r)).Select(r => JsonSerializer.Deserialize(r.Payload, types.Resolve(r.Type))
            ?? throw new InvalidOperationException($"Event {r.StreamId}:{r.Sequence} has no payload.")).ToList();
        var aggregate = new T();
        aggregate.LoadFromHistory(id, history, records[^1].Sequence);
        return aggregate;
    }

    private EventRecord Upcast(EventRecord record)
    {
        if (upcasts is null)
            return record;
        var (type, payload) = upcasts.Upcast(record.Type, record.Payload);
        return record with { Type = type, Payload = payload };
    }

    /// <inheritdoc />
    public async Task SaveAsync(T aggregate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        if (aggregate.UncommittedEvents.Count == 0)
            return;

        var streamId = StreamIdOf(aggregate.Id);
        var events = aggregate.UncommittedEvents
            .Select(e => new NewEvent(EventTypeRegistry.NameOf(e.GetType()), JsonSerializer.Serialize(e, e.GetType())))
            .ToList();
        var result = await store.AppendAsync(streamId, aggregate.Version, events, cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded)
            throw new EventStreamConflictException(streamId, aggregate.Version);
        aggregate.MarkCommitted(result.LastSequence);
    }
}
