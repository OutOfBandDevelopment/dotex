using System.Collections.Generic;

namespace OoBDev.Imaging.Analysis;

/// <summary>Per-channel and luminance counts.</summary>
/// <param name="Red">Counts per bin for red.</param>
/// <param name="Green">Counts per bin for green.</param>
/// <param name="Blue">Counts per bin for blue.</param>
/// <param name="Luminance">Counts per bin for Rec. 709 luminance.</param>
public sealed record Histograms(IReadOnlyList<int> Red, IReadOnlyList<int> Green, IReadOnlyList<int> Blue, IReadOnlyList<int> Luminance);
