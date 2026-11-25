using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing;

/// <summary>
/// The one way to ask for a data subject's personal data to be erased. A host puts its own right (for example <c>erasure.request</c>) in front of it;
/// the compaction job does the blanking later.
/// </summary>
public interface IErasureEntryPoint
{
    /// <summary>Records the request for <paramref name="subjectId"/> and returns the new request id.</summary>
    /// <param name="subjectId">The data subject.</param>
    /// <param name="knownCopies">Other places that hold copies and need their own request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The request id that <see cref="ErasureCompleted"/> will carry.</returns>
    Task<string> RequestAsync(string subjectId, IReadOnlyList<string>? knownCopies = null, CancellationToken cancellationToken = default);
}
