using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing;

/// <summary>
/// Catches projections up by reading the event store from each projection checkpoint (kept in the entity store),
/// and can rebuild one from the beginning.
/// </summary>
/// <param name="eventStore">The event store.</param>
/// <param name="entityStore">The entity store that holds the checkpoints.</param>
/// <param name="projections">The projections.</param>
public class ProjectionRunner(IEventStore eventStore, IEntityStore entityStore, IEnumerable<IProjection> projections)
{
    private const string CheckpointType = "projection-checkpoint";

    /// <summary>
    /// Applies events that are newer than each projection checkpoint, in batches, until all are current.
    /// </summary>
    /// <param name="batchSize">The most events read at once.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of event applications made.</returns>
    public async Task<int> CatchUpAsync(int batchSize = 500, CancellationToken cancellationToken = default)
    {
        var applied = 0;
        foreach (var projection in projections)
            applied += await CatchUpAsync(projection, batchSize, cancellationToken).ConfigureAwait(false);
        return applied;
    }

    /// <summary>
    /// Clears a projection checkpoint so the next catch-up replays every event. The projection clears its own read model.
    /// </summary>
    /// <param name="name">The projection name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task ResetAsync(string name, CancellationToken cancellationToken = default)
        => await entityStore.DeleteAsync(CheckpointType, name, null, cancellationToken).ConfigureAwait(false);

    private async Task<int> CatchUpAsync(IProjection projection, int batchSize, CancellationToken cancellationToken)
    {
        var applied = 0;
        while (true)
        {
            var checkpoint = await entityStore.GetAsync(CheckpointType, projection.Name, cancellationToken).ConfigureAwait(false);
            var position = checkpoint is null ? 0 : JsonSerializer.Deserialize<long>(checkpoint.Value);
            var batch = await eventStore.ReadAllAsync(position, batchSize, cancellationToken).ConfigureAwait(false);
            if (batch.Count == 0)
                return applied;

            foreach (var record in batch)
            {
                await projection.ApplyAsync(record, cancellationToken).ConfigureAwait(false);
                applied++;
            }

            var saved = await entityStore.SaveAsync(CheckpointType, projection.Name, JsonSerializer.Serialize(batch[^1].Position), checkpoint?.Version ?? 0, cancellationToken).ConfigureAwait(false);
            if (saved is null)
                throw new InvalidOperationException($"Projection '{projection.Name}' lost its checkpoint to another runner; run one runner per projection.");
        }
    }
}
