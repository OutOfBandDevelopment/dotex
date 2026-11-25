using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace OoBDev.Docling.Tests;

/// <summary>Word-level accuracy of recognised text against the known text (1.0 is identical).</summary>
internal static partial class WordAccuracy
{
    [GeneratedRegex("[a-z0-9]+")]
    private static partial Regex Word();

    /// <summary>One minus the word edit distance over the length of the truth.</summary>
    /// <param name="truth">The known text.</param>
    /// <param name="recognised">What OCR returned.</param>
    public static double Score(string truth, string recognised)
    {
        ArgumentNullException.ThrowIfNull(truth);
        ArgumentNullException.ThrowIfNull(recognised);
        var a = Word().Matches(truth.ToLowerInvariant()).Select(m => m.Value).ToArray();
        var b = Word().Matches(recognised.ToLowerInvariant()).Select(m => m.Value).ToArray();
        if (a.Length == 0)
        {
            return b.Length == 0 ? 1 : 0;
        }

        var previous = Enumerable.Range(0, b.Length + 1).ToArray();
        for (var i = 1; i <= a.Length; i++)
        {
            var current = new int[b.Length + 1];
            current[0] = i;
            for (var j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                current[j] = Math.Min(Math.Min(current[j - 1] + 1, previous[j] + 1), previous[j - 1] + cost);
            }

            previous = current;
        }

        return Math.Max(0, 1 - (double)previous[b.Length] / a.Length);
    }
}
