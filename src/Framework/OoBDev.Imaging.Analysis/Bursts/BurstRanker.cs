using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OoBDev.Imaging.Analysis.Bursts;

/// <summary>
/// Grouping and ranking of bursts as pure functions. The caller supplies capture times, the similarity links
/// (embedding or dHash) and the measures; nothing here reads or changes a file.
/// </summary>
public static class BurstRanker
{
    /// <summary>Default time window in seconds.</summary>
    public const double Window = 60.0;

    /// <summary>Default embedding similarity threshold.</summary>
    public const double Sim = 0.88;

    /// <summary>Default dHash distance (bits) threshold.</summary>
    public const int DHashBits = 10;

    /// <summary>How many following frames each frame is compared with.</summary>
    public const int Look = 4;

    /// <summary>Default largest group.</summary>
    public const int MaxGroup = 60;

    private static readonly Dictionary<string, double> Penalties = new(StringComparer.Ordinal)
    {
        ["shaky"] = 1.0,
        ["defocused"] = 0.5,
        ["over"] = 1.0,
        ["under"] = 1.0,
        ["noisy"] = 0.5,
        ["flat"] = 0.25,
    };

    /// <summary>The frame pairs (indexes in time order) taken within the window, each frame against the next <see cref="Look"/>.</summary>
    public static IEnumerable<(int I, int J)> Pairs(IReadOnlyList<DateTime> taken, double window)
    {
        ArgumentNullException.ThrowIfNull(taken);
        for (var i = 0; i < taken.Count; i++)
        {
            for (var j = i + 1; j < Math.Min(taken.Count, i + 1 + Look); j++)
            {
                if ((taken[j] - taken[i]).TotalSeconds > window)
                    break;
                yield return (i, j);
            }
        }
    }

    /// <summary>Transitive groups of two or more.</summary>
    public static IReadOnlyList<IReadOnlyList<int>> Components(int n, IEnumerable<(int A, int B)> links)
    {
        ArgumentNullException.ThrowIfNull(links);
        var parent = Enumerable.Range(0, n).ToArray();

        int Find(int x)
        {
            while (parent[x] != x)
            {
                parent[x] = parent[parent[x]];
                x = parent[x];
            }

            return x;
        }

        foreach (var (a, b) in links)
        {
            var ra = Find(a);
            var rb = Find(b);
            if (ra != rb)
                parent[ra] = rb;
        }

        var groups = new Dictionary<int, List<int>>();
        for (var i = 0; i < n; i++)
        {
            var root = Find(i);
            if (!groups.TryGetValue(root, out var group))
                groups[root] = group = [];
            group.Add(i);
        }

        return [.. groups.Values.Where(g => g.Count > 1)];
    }

    /// <summary>
    /// Members best first: the subject focus when every member has one, else the whole-frame focus, minus penalties
    /// for shake, exposure and noise. A member with no measurement scores -9.
    /// </summary>
    public static IReadOnlyList<BurstScore> Score(
        IReadOnlyList<string> members,
        IReadOnlyDictionary<string, QualityMeasures> measures,
        IReadOnlyDictionary<string, SubjectFocus> subjects)
    {
        ArgumentNullException.ThrowIfNull(members);
        ArgumentNullException.ThrowIfNull(measures);
        ArgumentNullException.ThrowIfNull(subjects);
        var useSubject = members.All(s => subjects.TryGetValue(s, out var f) && f.Count != 0);
        var basis = useSubject ? "subject" : "frame";
        var focus = new Dictionary<string, double>(StringComparer.Ordinal);
        foreach (var s in members)
        {
            if (useSubject)
            {
                if (subjects[s].FocusNorm is { } v)
                    focus[s] = v;
            }
            else if (measures.TryGetValue(s, out var m))
            {
                focus[s] = m.FocusTile;
            }
        }

        var top = focus.Values.Where(v => v > 0).DefaultIfEmpty(0).Max();
        var scores = new List<BurstScore>();
        foreach (var s in members)
        {
            if (!focus.TryGetValue(s, out var v) || top <= 0 || v <= 0)
            {
                scores.Add(new BurstScore(s, -9.0, basis, "no measurement"));
                continue;
            }

            var notes = new List<string>();
            var score = Math.Max(-4.0, Math.Log2(v / top));
            if (score < -0.1)
                notes.Add(string.Create(CultureInfo.InvariantCulture, $"focus {score:+0.0;-0.0}"));
            if (measures.TryGetValue(s, out var m))
            {
                foreach (var label in QualityLabels.Of(m))
                {
                    if (Penalties.TryGetValue(label, out var p))
                    {
                        score -= p;
                        notes.Add(string.Create(CultureInfo.InvariantCulture, $"{label} -{p:0.##}"));
                    }
                }
            }

            scores.Add(new BurstScore(s, Math.Round(score, 3, MidpointRounding.ToEven), basis, notes.Count == 0 ? "sharpest" : string.Join(", ", notes)));
        }

        return [.. scores.OrderByDescending(t => t.Score)];
    }
}
