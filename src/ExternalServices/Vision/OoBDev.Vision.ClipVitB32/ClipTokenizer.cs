using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace OoBDev.Vision.ClipVitB32;

/// <summary>
/// CLIP byte-pair-encoding tokenizer (<c>vocab.json</c> and <c>merges.txt</c> of the OpenAI model): lower case,
/// collapse whitespace, split with the CLIP pattern, byte-to-unicode map, merges with the end-of-word marker.
/// </summary>
public sealed partial class ClipTokenizer
{
    /// <summary>Id of <c>&lt;|startoftext|&gt;</c>.</summary>
    public const int StartOfText = 49406;

    /// <summary>Id of <c>&lt;|endoftext|&gt;</c>, also the padding id.</summary>
    public const int EndOfText = 49407;

    private const string EndOfWord = "</w>";

    private readonly Dictionary<string, int> _vocabulary;
    private readonly Dictionary<(string, string), int> _ranks;
    private readonly string[] _byteToUnicode = new string[256];
    private readonly Dictionary<string, int[]> _cache = [];
    private readonly object _cacheLock = new();

    /// <summary>Creates a tokenizer from the model's files.</summary>
    /// <param name="vocabularyJson">Content of <c>vocab.json</c>.</param>
    /// <param name="merges">Lines of <c>merges.txt</c> (the version header line is skipped).</param>
    public ClipTokenizer(string vocabularyJson, IEnumerable<string> merges)
    {
        ArgumentException.ThrowIfNullOrEmpty(vocabularyJson);
        ArgumentNullException.ThrowIfNull(merges);
        _vocabulary = JsonSerializer.Deserialize<Dictionary<string, int>>(vocabularyJson) ?? throw new InvalidDataException("vocab.json is empty.");
        _ranks = [];
        foreach (var line in merges)
        {
            if (line.Length == 0 || line.StartsWith("#version", StringComparison.Ordinal)) continue;
            var parts = line.Split(' ');
            if (parts.Length != 2) throw new InvalidDataException($"Bad merge line '{line}'.");
            _ranks.TryAdd((parts[0], parts[1]), _ranks.Count);
        }
        BuildByteMap();
    }

    /// <summary>Loads a tokenizer from a model folder.</summary>
    /// <param name="vocabularyPath">Path of <c>vocab.json</c>.</param>
    /// <param name="mergesPath">Path of <c>merges.txt</c>.</param>
    /// <returns>The tokenizer.</returns>
    public static ClipTokenizer Load(string vocabularyPath, string mergesPath) =>
        new(File.ReadAllText(vocabularyPath, Encoding.UTF8), File.ReadLines(mergesPath, Encoding.UTF8));

    /// <summary>
    /// Encodes text as <c>[start, tokens..., end]</c>, cut to <paramref name="maxLength"/> ids (the end token is kept).
    /// </summary>
    /// <param name="text">Text to encode.</param>
    /// <param name="maxLength">Largest number of ids, including start and end.</param>
    /// <returns>Token ids.</returns>
    public int[] Encode(string text, int maxLength = 77)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (maxLength < 3) throw new ArgumentOutOfRangeException(nameof(maxLength), "At least 3 ids are needed.");

        var ids = new List<int>(maxLength) { StartOfText };
        var clean = Whitespace().Replace(text, " ").Trim().ToLowerInvariant();
        foreach (Match match in Pattern().Matches(clean))
        {
            if (ids.Count >= maxLength - 1) break;
            var token = match.Value;
            if (token == "<|startoftext|>") ids.Add(StartOfText);
            else if (token == "<|endoftext|>") ids.Add(EndOfText);
            else
            {
                foreach (var id in Bpe(token))
                {
                    if (ids.Count >= maxLength - 1) break;
                    ids.Add(id);
                }
            }
        }
        ids.Add(EndOfText);
        return [.. ids];
    }

    private int[] Bpe(string token)
    {
        lock (_cacheLock)
        {
            if (_cache.TryGetValue(token, out var cached)) return cached;
        }

        var mapped = string.Concat(Encoding.UTF8.GetBytes(token).Select(b => _byteToUnicode[b]));
        var symbols = new List<string>();
        var elements = StringInfoElements(mapped);
        for (var i = 0; i < elements.Count; i++) symbols.Add(i == elements.Count - 1 ? elements[i] + EndOfWord : elements[i]);

        while (symbols.Count > 1)
        {
            var best = int.MaxValue;
            var at = -1;
            for (var i = 0; i < symbols.Count - 1; i++)
            {
                if (_ranks.TryGetValue((symbols[i], symbols[i + 1]), out var rank) && rank < best)
                {
                    best = rank;
                    at = i;
                }
            }
            if (at < 0) break;

            var (first, second) = (symbols[at], symbols[at + 1]);
            var merged = new List<string>(symbols.Count);
            for (var i = 0; i < symbols.Count; i++)
            {
                if (i < symbols.Count - 1 && symbols[i] == first && symbols[i + 1] == second)
                {
                    merged.Add(first + second);
                    i++;
                }
                else merged.Add(symbols[i]);
            }
            symbols = merged;
        }

        var result = symbols.Select(s => _vocabulary.TryGetValue(s, out var id) ? id : throw new InvalidDataException($"'{s}' is not in the vocabulary.")).ToArray();
        lock (_cacheLock)
        {
            if (_cache.Count > 50_000) _cache.Clear();
            _cache[token] = result;
        }
        return result;
    }

    /// <summary>The mapped text is made of single BMP characters, so every UTF-16 code unit is one element.</summary>
    private static List<string> StringInfoElements(string mapped) => [.. mapped.Select(c => c.ToString())];

    private void BuildByteMap()
    {
        // GPT-2 / CLIP bytes_to_unicode: printable bytes map to themselves, the rest to 256 + n.
        var printable = new List<int>();
        printable.AddRange(Enumerable.Range('!', '~' - '!' + 1));
        printable.AddRange(Enumerable.Range('¡', '¬' - '¡' + 1));
        printable.AddRange(Enumerable.Range('®', 'ÿ' - '®' + 1));
        var extra = 0;
        for (var b = 0; b < 256; b++)
            _byteToUnicode[b] = ((char)(printable.Contains(b) ? b : 256 + extra++)).ToString();
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    [GeneratedRegex(@"<\|startoftext\|>|<\|endoftext\|>|'s|'t|'re|'ve|'m|'ll|'d|[\p{L}]+|[\p{N}]|[^\s\p{L}\p{N}]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex Pattern();
}
