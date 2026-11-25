using System.Collections.Generic;

namespace OoBDev.Imaging.People;

/// <summary>
/// One face or pet found in an image.
/// </summary>
/// <param name="Kind">Face or pet.</param>
/// <param name="Box">Where it is.</param>
/// <param name="Score">The detector confidence, 0 to 1.</param>
/// <param name="Species">The species of a pet; null for a face.</param>
/// <param name="Landmarks">Landmarks the detector found; null when it gives none.</param>
public sealed record DetectedItem(MatchKind Kind, ImageBox Box, double Score, string? Species = null, IReadOnlyList<ImagePoint>? Landmarks = null);
