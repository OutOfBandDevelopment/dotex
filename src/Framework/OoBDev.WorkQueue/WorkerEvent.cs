using System;

namespace OoBDev.WorkQueue;

/// <summary>
/// One progress event of a worker, as kept in the event history.
/// </summary>
/// <param name="Id">The increasing id; readers keep the last one they saw.</param>
/// <param name="At">When the event happened.</param>
/// <param name="Source">Who published it (a stage or worker name).</param>
/// <param name="Message">The text of the event; carries ids and counts, never secrets.</param>
public sealed record WorkerEvent(long Id, DateTimeOffset At, string Source, string Message);
