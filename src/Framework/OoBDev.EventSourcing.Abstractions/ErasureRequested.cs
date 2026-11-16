using System.Collections.Generic;

namespace OoBDev.EventSourcing;

/// <summary>The tombstone appended to the subject's erasure stream; it never carries personal values.</summary>
/// <param name="SubjectId">The data subject.</param>
/// <param name="RequestId">The request id.</param>
/// <param name="KnownCopies">Other places that hold copies (backups, exports) and need their own request.</param>
[EventType("ErasureRequested")]
public sealed record ErasureRequested(string SubjectId, string RequestId, IReadOnlyList<string> KnownCopies);
