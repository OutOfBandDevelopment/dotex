using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OoBDev.Documents.Markdown;

/// <summary>
/// Page text to embedding chunks: split on blank lines, pack paragraphs up to <see cref="MaxChars"/>, cut longer paragraphs at a space with <see cref="Overlap"/> characters of overlap,
/// drop chunks under <see cref="MinChars"/>. Lengths count code points.
/// </summary>
public static partial class ChunkText
{
    /// <summary>The largest chunk.</summary>
    public const int MaxChars = 1500;

    /// <summary>The characters repeated at the start of the next piece of a cut paragraph.</summary>
    public const int Overlap = 150;

    /// <summary>The smallest chunk kept.</summary>
    public const int MinChars = 30;

    /// <summary>Splits <paramref name="text"/> into chunks.</summary>
    /// <param name="text">The page text.</param>
    public static IReadOnlyList<string> Split(string text)
    {
        var chunks = new List<string>();
        var current = "";
        var currentLength = 0;
        foreach (var raw in Paragraphs().Split(text))
        {
            var paragraph = raw.Trim();
            if (paragraph.Length == 0)
            {
                continue;
            }

            var runes = paragraph.EnumerateRunes().ToArray();
            while (runes.Length > MaxChars)
            {
                var cut = LastSpace(runes);
                if (cut <= 0)
                {
                    cut = MaxChars;
                }

                if (currentLength > 0)
                {
                    chunks.Add(current);
                    current = "";
                    currentLength = 0;
                }

                chunks.Add(Join(runes, cut));
                runes = runes[(cut - Overlap)..];
            }

            if (currentLength > 0 && currentLength + runes.Length + 2 > MaxChars)
            {
                chunks.Add(current);
                current = "";
                currentLength = 0;
            }

            var piece = Join(runes, runes.Length);
            current = currentLength > 0 ? $"{current}\n\n{piece}" : piece;
            currentLength = currentLength > 0 ? currentLength + 2 + runes.Length : runes.Length;
        }

        if (currentLength > 0)
        {
            chunks.Add(current);
        }

        return chunks.Where(c => c.EnumerateRunes().Count() >= MinChars).ToList();
    }

    private static int LastSpace(Rune[] runes)
    {
        for (var i = MaxChars - 1; i >= MaxChars / 2; i--)
        {
            if (runes[i].Value == ' ')
            {
                return i;
            }
        }

        return -1;
    }

    private static string Join(Rune[] runes, int count)
    {
        var sb = new StringBuilder(count);
        for (var i = 0; i < count; i++)
        {
            sb.Append(runes[i].ToString());
        }

        return sb.ToString();
    }

    [GeneratedRegex(@"\n\s*\n", RegexOptions.None, 1000)]
    private static partial Regex Paragraphs();
}
