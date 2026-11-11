using Microsoft.Extensions.Options;
using OoBDev.MessageQueueing;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing;

/// <summary>
/// Reads the event store by global position and sends the events through the message-queue sender,
/// saving its checkpoint in the entity store. Events are sent at least once; a consumer must be idempotent
/// (the message id is <c>{StreamId}:{Sequence}</c>).
/// </summary>
public sealed class EventRelay(
    IEventStore eventStore,
    IEntityStore entityStore,
    IMessageQueueSender<EventsChannel> sender,
    IOptions<EventRelayOptions> options,
    TimeProvider timeProvider)
{
    private const string CheckpointType = "checkpoint";

    /// <summary>
    /// Relays the events appended since the last checkpoint, one batch.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of events sent.</returns>
    public async Task<int> RunOnceAsync(CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        var checkpoint = await entityStore.GetAsync(CheckpointType, settings.Name, cancellationToken).ConfigureAwait(false);
        var position = checkpoint is null ? 0 : JsonSerializer.Deserialize<long>(checkpoint.Value);

        var batch = await eventStore.ReadAllAsync(position, settings.BatchSize, cancellationToken).ConfigureAwait(false);
        var cutoff = timeProvider.GetUtcNow() - settings.SafetyWindow;
        var ready = batch.TakeWhile(e => e.Timestamp <= cutoff).ToList();
        if (ready.Count == 0)
            return 0;

        foreach (var record in ready)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await sender.SendAsync(record, $"{record.StreamId}:{record.Sequence}").ConfigureAwait(false);
        }

        var last = ready[^1].Position;
        var saved = await entityStore.SaveAsync(CheckpointType, settings.Name, JsonSerializer.Serialize(last), checkpoint?.Version ?? 0, cancellationToken).ConfigureAwait(false);
        if (saved is null)
            throw new InvalidOperationException($"Relay '{settings.Name}' lost its checkpoint to another instance; run one relay per name.");
        return ready.Count;
    }
}
