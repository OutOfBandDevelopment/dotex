using System.Collections.Generic;

namespace OoBDev.Midi;

/// <summary>
/// One <c>MTrk</c> chunk: an ordered list of events.
/// </summary>
/// <param name="Events">The events in file order.</param>
public sealed record MidiTrack(IReadOnlyList<MidiEvent> Events);
