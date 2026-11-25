using System.Collections.Generic;

namespace OoBDev.DevTerm.Logging;

/// <summary>
/// What one engine call produced: the items played, in order, and whether the display should be
/// cleared first (<see cref="Reset"/>) because playback went back to the start — a rewind, a
/// backward seek, or a presenter change, all of which replay from record 0 so stateful presenters
/// (the ASCII line buffer, a protocol decoder mid-frame) see exactly the byte stream they saw live.
/// </summary>
public sealed record PlaybackBatch(bool Reset, IReadOnlyList<PlaybackItem> Items)
{
    public static PlaybackBatch Empty { get; } = new(false, []);
}
