using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OoBDev.System.Retro.Apple2.ApplesoftBASIC;

/// <summary>
/// Parses an Applesoft BASIC listing into lines, statements and elements (keywords and literal text). This is the lexical level the
/// interpreter itself works at; it does not build expression trees. Like the interpreter it ignores spaces outside strings, matches the
/// longest keyword at each position (so <c>SCORE</c> reads as <c>SC OR E</c>), and takes the rest of a line after <c>REM</c>, and a
/// <c>DATA</c> body up to the next colon outside quotes, literally.
/// </summary>
public class ApplesoftParser
{
    /// <summary>The longest line the Apple II input buffer accepts.</summary>
    public const int MaxLineLength = 239;

    /// <summary>The highest line number.</summary>
    public const int MaxLineNumber = 63999;

    /// <summary>The most characters accepted in one listing.</summary>
    public const int MaxListingLength = 1_048_576;

    private const byte _rem = 0xB2;
    private const byte _data = 0x83;
    private readonly TokenMap _map = new();

    /// <summary>
    /// Parses a listing, one line per row, each starting with its line number. Blank rows are skipped.
    /// </summary>
    /// <param name="listing">The program text.</param>
    /// <returns>The lines in listing order.</returns>
    /// <exception cref="FormatException">A row has no line number, a number is out of range, a row is too long, or the listing is too large.</exception>
    public IReadOnlyList<ApplesoftLine> ParseListing(string listing)
    {
        ArgumentNullException.ThrowIfNull(listing);
        if (listing.Length > MaxListingLength)
        {
            throw new FormatException($"The listing is longer than {MaxListingLength} characters.");
        }

        var lines = new List<ApplesoftLine>();
        foreach (var row in listing.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            lines.Add(ParseLine(row));
        }

        return lines;
    }

    /// <summary>
    /// Parses one numbered line, for example <c>10 PRINT "HI":GOTO 10</c>.
    /// </summary>
    /// <param name="row">The line text.</param>
    /// <returns>The parsed line.</returns>
    /// <exception cref="FormatException">The row has no line number, the number is out of range, or the row is too long.</exception>
    public ApplesoftLine ParseLine(string row)
    {
        ArgumentNullException.ThrowIfNull(row);
        row = row.Trim();
        if (row.Length > MaxLineLength)
        {
            throw new FormatException($"A line is longer than {MaxLineLength} characters.");
        }

        var digits = 0;
        while (digits < row.Length && char.IsAsciiDigit(row[digits]))
        {
            digits++;
        }

        if (digits == 0 || !int.TryParse(row.AsSpan(0, digits), NumberStyles.None, CultureInfo.InvariantCulture, out var number) || number > MaxLineNumber)
        {
            throw new FormatException($"A line must start with a line number from 0 to {MaxLineNumber}: '{Shorten(row)}'.");
        }

        return new ApplesoftLine(number, ParseStatements(row[digits..]));
    }

    private List<ApplesoftStatement> ParseStatements(string body)
    {
        var statements = new List<ApplesoftStatement>();
        var elements = new List<ApplesoftElement>();
        var literal = new StringBuilder();
        var i = 0;
        while (i < body.Length)
        {
            var c = body[i];
            if (char.IsWhiteSpace(c))
            {
                i++;
            }
            else if (c == '"')
            {
                i = TakeString(body, i, literal);
            }
            else if (c == ':')
            {
                Flush(literal, elements);
                statements.Add(new ApplesoftStatement(elements));
                elements = [];
                i++;
            }
            else if (MatchKeyword(body, i) is { } keyword)
            {
                Flush(literal, elements);
                elements.Add(new ApplesoftElement(keyword.Keyword, keyword.Code));
                i = keyword.End;
                if (keyword.Code == _rem)
                {
                    elements.Add(new ApplesoftElement(body[i..]));
                    i = body.Length;
                }
                else if (keyword.Code == _data)
                {
                    i = TakeData(body, i, elements);
                }
            }
            else
            {
                literal.Append(char.ToUpperInvariant(c));
                i++;
            }
        }

        Flush(literal, elements);
        statements.Add(new ApplesoftStatement(elements));
        return statements;
    }

    private static int TakeString(string body, int start, StringBuilder literal)
    {
        var end = body.IndexOf('"', start + 1);
        var stop = end < 0 ? body.Length : end + 1;
        literal.Append(body, start, stop - start);
        return stop;
    }

    private static int TakeData(string body, int start, List<ApplesoftElement> elements)
    {
        var i = start;
        var inQuotes = false;
        while (i < body.Length && (inQuotes || body[i] != ':'))
        {
            inQuotes ^= body[i] == '"';
            i++;
        }

        if (i > start)
        {
            elements.Add(new ApplesoftElement(body[start..i]));
        }

        return i;
    }

    private (string Keyword, byte Code, int End)? MatchKeyword(string body, int start)
    {
        foreach (var (code, keyword) in _map.Keywords)
        {
            var end = Consume(body, start, keyword);
            if (end > 0)
            {
                return (keyword, code, end);
            }
        }

        return null;
    }

    private static int Consume(string body, int start, string keyword)
    {
        var i = start;
        foreach (var expected in keyword)
        {
            while (i < body.Length && char.IsWhiteSpace(body[i]))
            {
                i++;
            }

            if (i >= body.Length || char.ToUpperInvariant(body[i]) != expected)
            {
                return -1;
            }

            i++;
        }

        return i;
    }

    private static void Flush(StringBuilder literal, List<ApplesoftElement> elements)
    {
        if (literal.Length > 0)
        {
            elements.Add(new ApplesoftElement(literal.ToString()));
            literal.Clear();
        }
    }

    private static string Shorten(string row) => row.Length <= 20 ? row : row[..20] + "...";
}
