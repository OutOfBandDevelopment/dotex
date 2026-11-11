using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing;

/// <summary>
/// Event-store backed repository: the stream id is <c>{AggregateType}-{id}</c> and events are stored as JSON.
/// Aggregates that implement <see cref="ISnapshotable"/> are snapshotted into the entity store (when one is supplied)
/// each <see cref="SnapshotOptions.Interval"/> events.
/// </summary>
/// <typeparam name="T">The aggregate type.</typeparam>
/// <param name="store">The event store.</param>
/// <param name="types">The event type registry.</param>
/// <param name="upcasts">The optional upcast pipeline.</param>
/// <param name="snapshots">The optional entity store that holds snapshots.</param>
/// <param name="snapshotOptions">The snapshot options.</param>
public class AggregateRepository<T>(
    [FromKeyedServices(EventSourcingGlobals.EventStoreSelectedKey)] IEventStore store,
    EventTypeRegistry types,
    EventUpcastPipeline? upcasts = null,
    [FromKeyedServices(EventSourcingGlobals.EntityStoreSelectedKey)] IEntityStore? snapshots = null,
    IOptions<SnapshotOptions>? snapshotOptions = null) : IAggregateRepository<T> where T : AggregateRoot, new()
{
    private const string SnapshotType = "aggregate-snapshot";

    private sealed record Snapshot(long Version, string State);

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
        var streamId = StreamIdOf(id);
        var aggregate = new T();
        long from = 1;
        var restored = false;

        if (aggregate is ISnapshotable snapshotable && snapshots is not null)
        {
            var stored = await snapshots.GetAsync(SnapshotType, streamId, cancellationToken).ConfigureAwait(false);
            var snapshot = stored is null ? null : JsonSerializer.Deserialize<Snapshot>(stored.Value);
            if (snapshot is not null)
            {
                snapshotable.RestoreState(snapshot.State);
                aggregate.LoadFromSnapshot(id, snapshot.Version);
                from = snapshot.Version + 1;
                restored = true;
            }
        }

        var records = await store.ReadAsync(streamId, from, cancellationToken).ConfigureAwait(false);
        if (records.Count == 0)
            return restored ? aggregate : null;

        var history = records.Select(r => Upcast(r)).Select(r => JsonSerializer.Deserialize(r.Payload, types.Resolve(r.Type))
            ?? throw new InvalidOperationException($"Event {r.StreamId}:{r.Sequence} has no payload.")).ToList();
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
        var previous = aggregate.Version;
        var events = aggregate.UncommittedEvents
            .Select(e => new NewEvent(EventTypeRegistry.NameOf(e.GetType()), JsonSerializer.Serialize(e, e.GetType())))
            .ToList();
        var result = await store.AppendAsync(streamId, previous, events, cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded)
            throw new EventStreamConflictException(streamId, previous);
        aggregate.MarkCommitted(result.LastSequence);

        await SnapshotAsync(aggregate, streamId, previous, cancellationToken).ConfigureAwait(false);
    }

    private async Task SnapshotAsync(T aggregate, string streamId, long previous, CancellationToken cancellationToken)
    {
        var interval = snapshotOptions?.Value.Interval ?? 0;
        if (snapshots is null || interval <= 0 || aggregate is not ISnapshotable snapshotable || aggregate.Version / interval == previous / interval)
            return;

        var current = await snapshots.GetAsync(SnapshotType, streamId, cancellationToken).ConfigureAwait(false);
        var value = JsonSerializer.Serialize(new Snapshot(aggregate.Version, snapshotable.CaptureState()));
        // A lost race only means another writer saved a snapshot; the stream stays the source of truth.
        await snapshots.SaveAsync(SnapshotType, streamId, value, current?.Version ?? 0, cancellationToken).ConfigureAwait(false);
    }
}
