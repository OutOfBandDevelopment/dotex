using System.Collections.Generic;
using OoBDev.Search.Requests;

namespace OoBDev.Search;

/// <summary>
/// Combines the ranked lists of several sources into one list (the outer join on the item key).
/// </summary>
public interface IScoreFusion
{
    /// <summary>The strategy name written into <see cref="SearchExplanation.Fusion"/>.</summary>
    string Name { get; }

    /// <summary>Fuses the inputs; an item missing from a source keeps no score for it. Result order is score descending, then item id.</summary>
    /// <param name="inputs">The per-source lists.</param>
    /// <returns>The fused items, each explaining every source score.</returns>
    IReadOnlyList<SearchItem> Fuse(IReadOnlyList<FusionInput> inputs);
}
