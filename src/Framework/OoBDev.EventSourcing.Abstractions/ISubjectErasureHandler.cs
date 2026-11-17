using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing;

/// <summary>
/// Told when a subject's personal data was erased, so a live read model can blank the same fields at once.
/// </summary>
public interface ISubjectErasureHandler
{
    /// <summary>Blanks the subject's personal data in the read model.</summary>
    /// <param name="subjectId">The erased subject.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SubjectErasedAsync(string subjectId, CancellationToken cancellationToken = default);
}
