using System;
using System.Collections.Generic;
using System.Linq;
using OoBDev.Search.Requests;

namespace OoBDev.Search.Fusion;

internal static class FusionMath
{
    /// <summary>Joins the inputs by item id; <paramref name="score"/> turns each source's (rank, item) into its contribution and <paramref name="combine"/> merges contributions.</summary>
    public static IReadOnlyList<SearchItem> Join(string fusion, IReadOnlyList<FusionInput> inputs, Func<FusionInput, int, SearchItem, double> score, Func<IEnumerable<double>, double> combine)
    {
        var byItem = new Dictionary<string, List<(FusionInput Input, SearchItem Item, double Contribution)>>(StringComparer.Ordinal);
        foreach (var input in inputs)
            for (var rank = 0; rank < input.Items.Count; rank++)
            {
                var item = input.Items[rank];
                if (!byItem.TryGetValue(item.ItemId, out var list))
                    byItem[item.ItemId] = list = [];
                list.Add((input, item, score(input, rank, item)));
            }

        return byItem
            .Select(pair =>
            {
                var scores = pair.Value.ToDictionary(p => p.Input.Source, p => p.Item.Score, StringComparer.Ordinal);
                var explanations = pair.Value.Select(p => p.Item.Explanation).ToArray();
                var explanation = new SearchExplanation(
                    explanations.Select(e => e.Field).FirstOrDefault(f => f is not null),
                    explanations.Select(e => e.VectorIndex).FirstOrDefault(v => v is not null),
                    explanations.Select(e => e.Distance).FirstOrDefault(d => d is not null),
                    scores,
                    fusion);
                var metaData = pair.Value.Select(p => p.Item.MetaData).FirstOrDefault(m => m is not null);
                return new SearchItem(pair.Key, combine(pair.Value.Select(p => p.Contribution)), explanation, metaData);
            })
            .OrderByDescending(i => i.Score)
            .ThenBy(i => i.ItemId, StringComparer.Ordinal)
            .ToArray();
    }
}
