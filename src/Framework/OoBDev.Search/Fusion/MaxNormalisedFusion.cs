using System.Collections.Generic;
using System.Linq;
using OoBDev.Search.Requests;

namespace OoBDev.Search.Fusion;

/// <summary>
/// Each source's scores are divided by that source's largest score; an item takes the largest normalised value of any source.
/// </summary>
public sealed class MaxNormalisedFusion : IScoreFusion
{
    /// <inheritdoc />
    public string Name => FusionNames.MaxNormalised;

    /// <inheritdoc />
    public IReadOnlyList<SearchItem> Fuse(IReadOnlyList<FusionInput> inputs)
    {
        var maxima = inputs.ToDictionary(i => i.Source, i => i.Items.Count == 0 ? 0 : i.Items.Max(x => x.Score));
        return FusionMath.Join(Name, inputs,
            (input, _, item) => maxima[input.Source] > 0 ? item.Score / maxima[input.Source] : 0,
            contributions => contributions.Max());
    }
}
