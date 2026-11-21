using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.ComplexEvents;
using OoBDev.WebApi.Models;

namespace OoBDev.WebApi.Services;

/// <summary>Schedules demo messages in the <see cref="IScheduleStore"/> and remembers what the runner handled, because the store has no listing.</summary>
/// <param name="store">The schedule store.</param>
/// <param name="time">The clock.</param>
public sealed class ScheduleDemo(IScheduleStore store, TimeProvider time)
{
    private const string EventType = "example.message";
    private const int MaxFired = 50;
    private readonly Lock _gate = new();
    private readonly Dictionary<string, ScheduleEntryModel> _pending = new(StringComparer.Ordinal);
    private readonly List<ScheduleEntryModel> _fired = [];

    /// <summary>Registers (or moves) an item.</summary>
    /// <param name="key">The key.</param>
    /// <param name="inSeconds">Seconds until due.</param>
    /// <param name="message">The event text.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    public async Task ScheduleAsync(string key, int inSeconds, string message, CancellationToken cancellationToken)
    {
        var due = time.GetUtcNow().AddSeconds(inSeconds);
        await store.RegisterAsync(key, due, EventType, Encoding.UTF8.GetBytes(message), cancellationToken).ConfigureAwait(false);
        lock (_gate)
        {
            _pending[key] = new ScheduleEntryModel(key, due, message, null);
        }
    }

    /// <summary>Removes a pending item.</summary>
    /// <param name="key">The key.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>False when there was none.</returns>
    public async Task<bool> ReleaseAsync(string key, CancellationToken cancellationToken)
    {
        var released = await store.ReleaseAsync(key, cancellationToken).ConfigureAwait(false);
        lock (_gate)
        {
            _pending.Remove(key);
        }

        return released;
    }

    /// <summary>The runner callback: records the item as fired.</summary>
    /// <param name="item">The due item.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>A completed task.</returns>
    public Task RunAsync(ScheduleItem item, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            _pending.Remove(item.Key);
            _fired.Insert(0, new ScheduleEntryModel(item.Key, item.DueAt, Encoding.UTF8.GetString(item.Payload.Span), time.GetUtcNow()));
            if (_fired.Count > MaxFired)
            {
                _fired.RemoveRange(MaxFired, _fired.Count - MaxFired);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>The current pending and fired items.</summary>
    /// <returns>The state.</returns>
    public ScheduleStateModel State()
    {
        lock (_gate)
        {
            return new ScheduleStateModel([.. _pending.Values.OrderBy(p => p.DueAt)], [.. _fired]);
        }
    }
}
