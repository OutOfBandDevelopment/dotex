using OoBDev.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace OoBDev.WebApi.Services;

/// <summary>
/// Scores passages for the search example: keyword ranking (BM25) and reciprocal rank fusion of ranked lists.
/// </summary>
public static partial class SearchScoring
{
    private const double K1 = 1.2;
    private const double B = 0.75;

    /// <summary>Rank constant of reciprocal rank fusion; 60 is the value from the original paper.</summary>
    public const int RankConstant = 60;

    /// <summary>Splits text into lower-case word tokens.</summary>
    /// <param name="text">The text.</param>
    /// <returns>The tokens in order.</returns>
    public static IReadOnlyList<string> Tokenize(string text)
        => WordPattern().Matches(text.ToLowerInvariant()).Select(m => m.Value).ToList();

    /// <summary>Ranks the passages against the query by BM25; passages sharing no term with the query are left out.</summary>
    /// <param name="query">The query.</param>
    /// <param name="passages">The passages.</param>
    /// <returns>Hits, best first.</returns>
    public static IList<SearchHitModel> Lexical(string query, IList<RagPassage> passages)
    {
        var terms = Tokenize(query).Distinct().ToList();
        var docs = passages.Select(p => (Passage: p, Tokens: Tokenize(p.Text))).ToList();
        if (terms.Count == 0 || docs.Count == 0)
        {
            return [];
        }

        var average = docs.Average(d => d.Tokens.Count);
        var hits = new List<SearchHitModel>();
        foreach (var (passage, tokens) in docs)
        {
            double score = 0;
            foreach (var term in terms)
            {
                var frequency = tokens.Count(t => t == term);
                if (frequency == 0)
                {
                    continue;
                }

                var containing = docs.Count(d => d.Tokens.Contains(term));
                var idf = Math.Log(1 + ((docs.Count - containing + 0.5) / (containing + 0.5)));
                score += idf * frequency * (K1 + 1) / (frequency + (K1 * (1 - B + (B * tokens.Count / average))));
            }

            if (score > 0)
            {
                hits.Add(new SearchHitModel { Id = passage.Id, Text = passage.Text, Score = score });
            }
        }

        return hits.OrderByDescending(h => h.Score).ToList();
    }

    /// <summary>Merges ranked lists by reciprocal rank fusion: each list adds 1 / (60 + rank) for every passage it holds.</summary>
    /// <param name="lists">The ranked lists, best first.</param>
    /// <returns>Hits, best first.</returns>
    public static IList<SearchHitModel> Fuse(params IList<SearchHitModel>[] lists)
    {
        var scores = new Dictionary<string, (SearchHitModel Hit, double Score)>();
        foreach (var list in lists)
        {
            for (var rank = 0; rank < list.Count; rank++)
            {
                var hit = list[rank];
                var add = 1.0 / (RankConstant + rank + 1);
                scores[hit.Id] = scores.TryGetValue(hit.Id, out var existing) ? (existing.Hit, existing.Score + add) : (hit, add);
            }
        }

        return scores.Values
            .OrderByDescending(v => v.Score)
            .Select(v => new SearchHitModel { Id = v.Hit.Id, Text = v.Hit.Text, Score = v.Score })
            .ToList();
    }

    [GeneratedRegex(@"[\p{L}\p{N}]+")]
    private static partial Regex WordPattern();
}
