using System.Collections.Generic;

namespace OoBDev.Search.Requests;

/// <summary>
/// Why an item matched.
/// </summary>
/// <param name="Field">The field or vector space that matched, when known.</param>
/// <param name="VectorIndex">Which of the item's vectors matched (for example the page), when the match was by vector.</param>
/// <param name="Distance">The raw distance of that vector, when known.</param>
/// <param name="Scores">The score each source gave the item, by source name; a hybrid result lists every source.</param>
/// <param name="Fusion">The fusion strategy that combined the sources, when the result is hybrid.</param>
public sealed record SearchExplanation(string? Field = null, int? VectorIndex = null, double? Distance = null, IReadOnlyDictionary<string, double>? Scores = null, string? Fusion = null);
