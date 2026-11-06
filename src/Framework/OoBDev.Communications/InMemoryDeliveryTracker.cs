using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.Communications.Abstractions;

namespace OoBDev.Communications;

/// <summary>Process-local <see cref="IDeliveryTracker"/>; state is lost on restart.</summary>
public sealed class InMemoryDeliveryTracker(TimeProvider timeProvider) : IDeliveryTracker
{
    private readonly ConcurrentDictionary<(string Channel, string MessageId), (SendResult Result, DeliveryStatusRecord Record)> _entries = new();

    /// <inheritdoc />
    public Task RecordSentAsync(string messageId, string channelName, SendResult result, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        var now = timeProvider.GetUtcNow();
        var record = new DeliveryStatusRecord(
            messageId,
            channelName,
            result.CorrelationId,
            result.Success ? DeliveryStatus.Sent : DeliveryStatus.Failed,
            result.Failure?.Message,
            now,
            result.Success ? result.Timestamp : null,
            result.Success ? null : result.Timestamp);
        _entries[(channelName, messageId)] = (result, record);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<SendResult?> GetSentResultAsync(string messageId, string channelName, CancellationToken cancellationToken = default)
        => Task.FromResult(_entries.TryGetValue((channelName, messageId), out var entry) ? entry.Result : null);

    /// <inheritdoc />
    public Task<DeliveryStatusRecord?> GetStatusAsync(string messageId, CancellationToken cancellationToken = default)
        => Task.FromResult(_entries
            .Where(e => string.Equals(e.Key.MessageId, messageId, StringComparison.Ordinal))
            .Select(e => e.Value.Record)
            .OrderByDescending(r => r.QueuedAt)
            .FirstOrDefault());

    /// <inheritdoc />
    public Task<bool> UpdateStatusAsync(DeliveryStatusUpdate update, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(update);
        var changed = false;
        foreach (var key in _entries.Keys.Where(k => string.Equals(k.MessageId, update.MessageId, StringComparison.Ordinal)))
        {
            _entries.AddOrUpdate(
                key,
                _ => throw new InvalidOperationException(),
                (_, entry) =>
                {
                    if (update.NewStatus <= entry.Record.Status)
                    {
                        return entry;
                    }

                    changed = true;
                    return (entry.Result, entry.Record with { Status = update.NewStatus, StatusDetails = update.StatusDetails ?? entry.Record.StatusDetails });
                });
        }

        return Task.FromResult(changed);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<DeliveryStatusRecord>> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<DeliveryStatusRecord>>(_entries.Values
            .Select(e => e.Record)
            .Where(r => string.Equals(r.CorrelationId, correlationId, StringComparison.Ordinal))
            .OrderBy(r => r.QueuedAt)
            .ToList());
}
