using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing;

/// <summary>
/// Event store that keeps events in memory; for tests and single-process use.
/// </summary>
public sealed class InMemoryEventStore(TimeProvider timeProvider) : IEventStore, IEventPayloadRewriter
{
    private readonly Lock _gate = new();
    private readonly List<EventRecord> _all = [];
    private readonly Dictionary<string, List<EventRecord>> _streams = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public Task<AppendResult> AppendAsync(string streamId, long expectedSequence, IReadOnlyList<NewEvent> events, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(streamId);
        ArgumentNullException.ThrowIfNull(events);
        lock (_gate)
        {
            var stream = _streams.TryGetValue(streamId, out var existing) ? existing : null;
            var last = stream?.Count ?? 0;
            if (last != expectedSequence)
                return Task.FromResult(new AppendResult(false, last, []));

            if (stream is null)
                _streams[streamId] = stream = [];

            var now = timeProvider.GetUtcNow();
            var stored = new List<EventRecord>(events.Count);
            foreach (var item in events)
            {
                var record = new EventRecord(streamId, stream.Count + 1, item.Type, now, item.Payload, item.Metadata, _all.Count + 1);
                stream.Add(record);
                _all.Add(record);
                stored.Add(record);
            }
            return Task.FromResult(new AppendResult(true, stream.Count, stored));
        }
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<EventRecord>> ReadAsync(string streamId, long fromSequence = 1, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            IReadOnlyList<EventRecord> result = _streams.TryGetValue(streamId, out var stream)
                ? [.. stream.Where(e => e.Sequence >= fromSequence)]
                : [];
            return Task.FromResult(result);
        }
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<EventRecord>> ReadAllAsync(long afterPosition, int maxCount = 500, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            IReadOnlyList<EventRecord> result = [.. _all.Where(e => e.Position > afterPosition).Take(maxCount)];
            return Task.FromResult(result);
        }
    }

    /// <inheritdoc />
    public Task<int> RewritePayloadsAsync(string streamId, IReadOnlyDictionary<long, string> payloads, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payloads);
        lock (_gate)
        {
            if (!_streams.TryGetValue(streamId, out var stream))
                return Task.FromResult(0);
            var changed = 0;
            foreach (var (sequence, payload) in payloads)
            {
                var index = (int)sequence - 1;
                if (index < 0 || index >= stream.Count || stream[index].Payload == payload)
                    continue;
                var updated = stream[index] with { Payload = payload };
                stream[index] = updated;
                _all[(int)updated.Position - 1] = updated;
                changed++;
            }

            return Task.FromResult(changed);
        }
    }
}
