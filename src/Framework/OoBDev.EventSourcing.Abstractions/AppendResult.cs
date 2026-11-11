using System.Collections.Generic;

namespace OoBDev.EventSourcing;

/// <summary>
/// The result of an append.
/// </summary>
/// <param name="Succeeded">False when the expected sequence did not match (optimistic concurrency conflict).</param>
/// <param name="LastSequence">The last sequence of the stream after the call (the current one on a conflict).</param>
/// <param name="Events">The stored events; empty on a conflict.</param>
public sealed record AppendResult(bool Succeeded, long LastSequence, IReadOnlyList<EventRecord> Events);
