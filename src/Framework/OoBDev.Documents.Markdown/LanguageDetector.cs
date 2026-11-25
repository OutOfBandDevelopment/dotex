using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace OoBDev.Documents.Markdown;

/// <summary>
/// A small stop-word language guess for recognised text, so a document can carry a language tag without a model or a package.
/// It knows a handful of Latin-script languages and answers <see langword="null"/> when no language leads clearly, which is the honest result for short or mixed text.
/// </summary>
public static class LanguageDetector
{
    private const int MinWords = 20;
    private const double MinShare = 0.08;
    private const double Lead = 1.5;

    private static readonly FrozenDictionary<string, FrozenSet<string>> Languages = new Dictionary<string, FrozenSet<string>>(StringComparer.Ordinal)
    {
        ["en"] = Set("the of and to in is that it for with as was on are this be by at from or an have not but they which"),
        ["de"] = Set("der die das und ist nicht ein eine zu den mit von sie auf für dem im auch sich es als wird wie aber oder nach"),
        ["fr"] = Set("le la les des et est une un du que qui dans pour pas sur au avec ce il elle sont par plus ne se mais ou"),
        ["es"] = Set("el la los las de y que en un una es por con para no se su al lo como más pero sus le ya o este"),
        ["it"] = Set("il lo la gli le di e che in un una è per con non si da come più ma anche sono del della dei al nel"),
        ["nl"] = Set("de het een en van in is dat op te zijn voor met niet aan er maar om ook als bij dan uit nog wordt"),
        ["pt"] = Set("o a os as de e que em um uma é para com não se por mais mas como foi ao do da dos das no na"),
    }.ToFrozenDictionary(StringComparer.Ordinal);

    /// <summary>Guesses the language of a text.</summary>
    /// <param name="text">The text.</param>
    /// <returns>An ISO 639-1 code, or <see langword="null"/> when the text is too short or no language leads clearly.</returns>
    public static string? Detect(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var hits = new Dictionary<string, int>(StringComparer.Ordinal);
        var words = 0;
        foreach (var word in Words(text))
        {
            words++;
            foreach (var (code, stops) in Languages)
            {
                if (stops.Contains(word))
                {
                    hits[code] = hits.GetValueOrDefault(code) + 1;
                }
            }
        }

        if (words < MinWords)
        {
            return null;
        }

        string? best = null;
        var bestHits = 0;
        var second = 0;
        foreach (var (code, count) in hits)
        {
            if (count > bestHits)
            {
                second = bestHits;
                (best, bestHits) = (code, count);
            }
            else if (count > second)
            {
                second = count;
            }
        }

        return best is not null && bestHits >= words * MinShare && bestHits >= second * Lead ? best : null;
    }

    private static IEnumerable<string> Words(string text)
    {
        var start = -1;
        for (var i = 0; i <= text.Length; i++)
        {
            var letter = i < text.Length && char.IsLetter(text[i]);
            if (letter && start < 0)
            {
                start = i;
            }
            else if (!letter && start >= 0)
            {
                yield return text[start..i].ToLowerInvariant();
                start = -1;
            }
        }
    }

    private static FrozenSet<string> Set(string words) => words.Split(' ').ToFrozenSet(StringComparer.Ordinal);
}
