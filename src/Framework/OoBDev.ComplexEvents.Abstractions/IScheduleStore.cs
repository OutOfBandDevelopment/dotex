using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents;

/// <summary>Persists scheduled events with leases so two instances never run the same item.</summary>
public interface IScheduleStore
{
    /// <summary>Adds an item, or moves the due time of the item with the same key.</summary>
    Task<ScheduleItem> RegisterAsync(string key, DateTimeOffset dueAt, string eventType, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken);

    /// <summary>Leases up to <paramref name="max"/> due items; an expired lease makes an item due again.</summary>
    Task<IReadOnlyList<ScheduleItem>> LeaseDueAsync(DateTimeOffset now, int max, TimeSpan lease, CancellationToken cancellationToken);

    /// <summary>Records completion; the item is not leased again.</summary>
    Task CompleteAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Removes a pending item by key; false when none.</summary>
    Task<bool> ReleaseAsync(string key, CancellationToken cancellationToken);
}
