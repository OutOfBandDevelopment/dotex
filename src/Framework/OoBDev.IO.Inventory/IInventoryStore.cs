using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.IO.Inventory;

/// <summary>Where scan results are kept; consumers bring their own store.</summary>
public interface IInventoryStore
{
    /// <summary>Finds the entry for a path, or null.</summary>
    /// <param name="path">Full path.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    ValueTask<InventoryEntry?> FindAsync(string path, CancellationToken cancellationToken);

    /// <summary>Adds or replaces an entry.</summary>
    /// <param name="entry">The entry.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    ValueTask SaveAsync(InventoryEntry entry, CancellationToken cancellationToken);

    /// <summary>Lists every entry.</summary>
    /// <param name="cancellationToken">Cancellation.</param>
    IAsyncEnumerable<InventoryEntry> ListAsync(CancellationToken cancellationToken);
}
