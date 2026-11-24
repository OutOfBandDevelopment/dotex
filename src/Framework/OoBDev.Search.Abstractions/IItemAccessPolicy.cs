using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Search;

/// <summary>Decides which indexed items the current caller may read, so search never returns a document the caller could not open.</summary>
public interface IItemAccessPolicy
{
    /// <summary>Returns the subset of the item ids the current caller may read.</summary>
    /// <param name="itemIds">The ids found by a search.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The readable ids; an id that is not returned is hidden.</returns>
    Task<IReadOnlySet<string>> FilterReadableAsync(IReadOnlyCollection<string> itemIds, CancellationToken cancellationToken = default);
}
