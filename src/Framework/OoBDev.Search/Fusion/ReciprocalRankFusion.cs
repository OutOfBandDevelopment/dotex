using System.Collections.Generic;
using System.Linq;
using OoBDev.Search.Requests;

namespace OoBDev.Search.Fusion;

/// <summary>
/// Sums <c>1 / (k + rank)</c> over the sources an item appears in (rank starts at 1); ignores the raw score scales.
/// </summary>
/// <param name="k">The damping constant; larger values flatten the difference between ranks.</param>
public sealed class ReciprocalRankFusion(int k = 60) : IScoreFusion
{
    /// <inheritdoc />
    public string Name => FusionNames.ReciprocalRank;

    /// <inheritdoc />
    public IReadOnlyList<SearchItem> Fuse(IReadOnlyList<FusionInput> inputs) =>
        FusionMath.Join(Name, inputs, (_, rank, _) => 1.0 / (k + rank + 1), contributions => contributions.Sum());
}
