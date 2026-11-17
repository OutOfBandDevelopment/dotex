using System.ComponentModel.DataAnnotations;

namespace OoBDev.Search.Fusion;

/// <summary>
/// Chooses the hybrid fusion strategy.
/// </summary>
public sealed class SearchFusionOptions
{
    /// <summary>The configuration section the options bind from.</summary>
    public const string SectionName = "Search:Fusion";

    /// <summary>The strategy: <see cref="FusionNames.MaxNormalised"/> or <see cref="FusionNames.ReciprocalRank"/>.</summary>
    [RegularExpression("^(max-normalised|reciprocal-rank)$")]
    public string Strategy { get; set; } = FusionNames.MaxNormalised;

    /// <summary>The reciprocal rank constant.</summary>
    [Range(1, 1000)]
    public int K { get; set; } = 60;
}
