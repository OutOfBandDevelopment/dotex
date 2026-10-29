using System.Collections.Generic;

namespace OoBDev.ZWave;

/// <summary>
/// What one frame means: a line of text, plus named values for indicators.
/// </summary>
/// <param name="Text">The readable line.</param>
/// <param name="Values">Named values the frame carries, for example <c>homeId</c> or <c>library</c>.</param>
public sealed record ZWaveReading(string Text, IReadOnlyDictionary<string, string> Values);
