using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.Services;

/// <summary>
/// Holds messages until they are published. A database implementation stores the entry in the same transaction
/// as the business change, which is what makes publish-after-commit safe.
/// </summary>
public interface IOutboxStore
{
    /// <summary>Stores a message for later publishing.</summary>
    /// <param name="entry">The entry.</param>
    Task AddAsync(OutboxEntry entry);

    /// <summary>Gets the oldest entries not yet marked sent.</summary>
    /// <param name="max">The most entries to return.</param>
    /// <returns>The pending entries, oldest first.</returns>
    Task<IReadOnlyList<OutboxEntry>> GetPendingAsync(int max);

    /// <summary>Marks an entry as published so it is not sent again.</summary>
    /// <param name="id">The entry id.</param>
    Task MarkSentAsync(string id);
}
