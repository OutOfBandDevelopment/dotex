using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace OoBDev.Onnx.SentenceEmbeddings;

/// <summary>
/// BERT tokenizer for single sentences: the reference basic tokenizer (clean, split CJK, optional lower case and accent
/// stripping, split on punctuation) followed by greedy longest-match WordPiece. Matches the Hugging Face
/// <c>BertTokenizer</c> used by sentence-transformers. Instances are immutable and safe to share.
/// </summary>
internal sealed class SentenceTokenizer
{
    private const int MaxCharsPerWord = 100;
    private const string ContinuationPrefix = "##";
    private static readonly string[] _specialTokens = ["[CLS]", "[SEP]", "[UNK]", "[PAD]", "[MASK]"];

    private readonly Dictionary<string, int> _vocabulary;
    private readonly bool _lowerCase;
    private readonly int _maxSequenceLength;
    private readonly int _clsId;
    private readonly int _sepId;
    private readonly int _unkId;

    public SentenceTokenizer(string vocabPath, bool lowerCase, int maxSequenceLength)
    {
        _vocabulary = new Dictionary<string, int>(StringComparer.Ordinal);
        var id = 0;
        foreach (var line in File.ReadLines(vocabPath, Encoding.UTF8))
            _vocabulary[line.TrimEnd('\r', '\n')] = id++;

        _lowerCase = lowerCase;
        _maxSequenceLength = maxSequenceLength;
        _clsId = Required("[CLS]");
        _sepId = Required("[SEP]");
        _unkId = Required("[UNK]");
    }

    private int Required(string token) =>
        _vocabulary.TryGetValue(token, out var id) ? id : throw new InvalidDataException($"The vocabulary has no {token} token.");

    /// <summary>
    /// Token ids for one sentence, including the start and end markers, at most <c>maxSequenceLength</c> long.
    /// </summary>
    public IReadOnlyList<int> Encode(string text)
    {
        var ids = new List<int>(Math.Min(_maxSequenceLength, text.Length + 2)) { _clsId };
        var limit = _maxSequenceLength - 1; // room for the closing marker

        var position = 0;
        while (position <= text.Length && ids.Count < limit)
        {
            var (special, index) = FindSpecialToken(text, position);
            var end = special is null ? text.Length : index;
            AppendText(text.AsSpan(position, end - position), ids, limit);
            if (special is null) break;
            if (ids.Count < limit) ids.Add(_vocabulary[special]);
            position = end + special.Length;
        }

        ids.Add(_sepId);
        return ids;
    }

    private static (string? Token, int Index) FindSpecialToken(string text, int start)
    {
        string? found = null;
        var foundAt = -1;
        foreach (var token in _specialTokens)
        {
            var at = text.IndexOf(token, start, StringComparison.Ordinal);
            if (at >= 0 && (foundAt < 0 || at < foundAt)) { found = token; foundAt = at; }
        }
        return (found, foundAt);
    }

    private void AppendText(ReadOnlySpan<char> text, List<int> ids, int limit)
    {
        var word = new StringBuilder();
        var index = 0;
        while (index < text.Length && ids.Count < limit)
        {
            Rune.DecodeFromUtf16(text[index..], out var rune, out var consumed);
            index += consumed;
            var value = rune.Value;

            if (value == 0 || value == 0xFFFD || IsControl(rune)) continue;

            if (IsWhitespace(rune)) { Flush(word, ids, limit); continue; }

            if (IsChineseCharacter(value))
            {
                Flush(word, ids, limit);
                word.Append(rune.ToString());
                Flush(word, ids, limit);
                continue;
            }

            if (IsPunctuation(rune))
            {
                Flush(word, ids, limit);
                word.Append(rune.ToString());
                Flush(word, ids, limit);
                continue;
            }

            word.Append(rune.ToString());
        }
        Flush(word, ids, limit);
    }

    private void Flush(StringBuilder word, List<int> ids, int limit)
    {
        if (word.Length == 0) return;
        var token = word.ToString();
        word.Clear();

        if (_lowerCase) token = StripAccents(token.ToLowerInvariant());

        // normalising can leave several pieces (e.g. a mark that decomposes); split them on punctuation again
        var piece = new StringBuilder();
        foreach (var rune in token.EnumerateRunes())
        {
            if (IsPunctuation(rune))
            {
                WordPiece(piece, ids, limit);
                piece.Append(rune.ToString());
                WordPiece(piece, ids, limit);
            }
            else
            {
                piece.Append(rune.ToString());
            }
        }
        WordPiece(piece, ids, limit);
    }

    private void WordPiece(StringBuilder piece, List<int> ids, int limit)
    {
        if (piece.Length == 0) return;
        var word = piece.ToString();
        piece.Clear();
        if (ids.Count >= limit) return;

        if (word.Length > MaxCharsPerWord)
        {
            ids.Add(_unkId);
            return;
        }

        var pieces = new List<int>();
        var start = 0;
        while (start < word.Length)
        {
            var end = word.Length;
            var current = -1;
            while (start < end)
            {
                var candidate = word.Substring(start, end - start);
                if (start > 0) candidate = ContinuationPrefix + candidate;
                if (_vocabulary.TryGetValue(candidate, out var id)) { current = id; break; }
                end--;
                if (end > start && char.IsLowSurrogate(word[end])) end--; // never cut a surrogate pair
            }

            if (current < 0)
            {
                ids.Add(_unkId); // the whole word is unknown
                return;
            }
            pieces.Add(current);
            start = end;
        }

        foreach (var id in pieces)
        {
            if (ids.Count >= limit) return;
            ids.Add(id);
        }
    }

    private static string StripAccents(string text)
    {
        var decomposed = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var rune in decomposed.EnumerateRunes())
            if (Rune.GetUnicodeCategory(rune) != UnicodeCategory.NonSpacingMark) builder.Append(rune.ToString());
        return builder.ToString();
    }

    private static bool IsWhitespace(Rune rune) =>
        rune.Value is ' ' or '\t' or '\n' or '\r' || Rune.GetUnicodeCategory(rune) == UnicodeCategory.SpaceSeparator;

    private static bool IsControl(Rune rune)
    {
        if (rune.Value is '\t' or '\n' or '\r') return false;
        var category = Rune.GetUnicodeCategory(rune);
        return category is UnicodeCategory.Control or UnicodeCategory.Format;
    }

    private static bool IsPunctuation(Rune rune)
    {
        var value = rune.Value;
        if (value is >= 33 and <= 47 or >= 58 and <= 64 or >= 91 and <= 96 or >= 123 and <= 126) return true;
        return Rune.GetUnicodeCategory(rune) is UnicodeCategory.ConnectorPunctuation or UnicodeCategory.DashPunctuation
            or UnicodeCategory.OpenPunctuation or UnicodeCategory.ClosePunctuation or UnicodeCategory.InitialQuotePunctuation
            or UnicodeCategory.FinalQuotePunctuation or UnicodeCategory.OtherPunctuation;
    }

    private static bool IsChineseCharacter(int value) =>
        value is >= 0x4E00 and <= 0x9FFF
            or >= 0x3400 and <= 0x4DBF
            or >= 0x20000 and <= 0x2A6DF
            or >= 0x2A700 and <= 0x2B73F
            or >= 0x2B740 and <= 0x2B81F
            or >= 0x2B820 and <= 0x2CEAF
            or >= 0xF900 and <= 0xFAFF
            or >= 0x2F800 and <= 0x2FA1F;
}
