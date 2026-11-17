using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.Search.Evaluation;

/// <summary>
/// Scores a ranked result list against the ids a person marked as relevant, so a set of queries with
/// expected hits can gate a change to ranking, fusion or an embedding model.
/// </summary>
public static class RelevanceMetrics
{
    /// <summary>The share of the relevant ids found in the first <paramref name="k"/> results (0 to 1).</summary>
    /// <param name="ranked">Result ids, best first.</param>
    /// <param name="relevant">The ids that should be found.</param>
    /// <param name="k">How many leading results count.</param>
    /// <returns>Recall at k; 1 when nothing is relevant.</returns>
    public static double RecallAtK(IReadOnlyList<string> ranked, IReadOnlySet<string> relevant, int k)
    {
        ArgumentNullException.ThrowIfNull(ranked);
        ArgumentNullException.ThrowIfNull(relevant);
        ArgumentOutOfRangeException.ThrowIfLessThan(k, 1);
        if (relevant.Count == 0)
        {
            return 1;
        }

        return (double)ranked.Take(k).Distinct(StringComparer.Ordinal).Count(relevant.Contains) / relevant.Count;
    }

    /// <summary>1 divided by the rank of the first relevant result; 0 when none is found.</summary>
    /// <param name="ranked">Result ids, best first.</param>
    /// <param name="relevant">The ids that should be found.</param>
    /// <returns>The reciprocal rank.</returns>
    public static double ReciprocalRank(IReadOnlyList<string> ranked, IReadOnlySet<string> relevant)
    {
        ArgumentNullException.ThrowIfNull(ranked);
        ArgumentNullException.ThrowIfNull(relevant);
        for (var i = 0; i < ranked.Count; i++)
        {
            if (relevant.Contains(ranked[i]))
            {
                return 1.0 / (i + 1);
            }
        }

        return 0;
    }

    /// <summary>The mean reciprocal rank over a set of queries.</summary>
    /// <param name="cases">Per query: the ranked result ids and the relevant ids.</param>
    /// <returns>The mean, or 0 for an empty set.</returns>
    public static double MeanReciprocalRank(IEnumerable<(IReadOnlyList<string> Ranked, IReadOnlySet<string> Relevant)> cases)
    {
        ArgumentNullException.ThrowIfNull(cases);
        var scores = cases.Select(c => ReciprocalRank(c.Ranked, c.Relevant)).ToList();
        return scores.Count == 0 ? 0 : scores.Average();
    }
}
