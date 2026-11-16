namespace OoBDev.EventSourcing;

/// <summary>Appended when the compaction job has blanked the subject's events.</summary>
/// <param name="SubjectId">The data subject.</param>
/// <param name="RequestId">The request id.</param>
/// <param name="EventsChanged">How many events were changed.</param>
[EventType("ErasureCompleted")]
[NoPersonalData]
public sealed record ErasureCompleted(string SubjectId, string RequestId, int EventsChanged);
