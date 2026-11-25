using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace OoBDev.DataLoader;

/// <summary>Reads CSV with a header row; supports quoted fields with commas, doubled quotes and line breaks.</summary>
public sealed class CsvRowReader : IRowReader
{
    /// <inheritdoc />
    public string Format => "csv";

    /// <inheritdoc />
    public async IAsyncEnumerable<SourceRow> ReadAsync(Stream source, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(source, Encoding.UTF8, true, 4096, leaveOpen: true);
        string[]? header = null;
        var lineNo = 0;
        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false)) is not null)
        {
            lineNo++;
            var startLine = lineNo;
            while (OpenQuote(line))
            {
                var next = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
                if (next is null) break;
                lineNo++;
                line += "\n" + next;
            }

            if (line.Length == 0) continue;
            var fields = Split(line);
            if (header is null)
            {
                header = fields;
                continue;
            }

            var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < header.Length; i++) values[header[i]] = i < fields.Length ? fields[i] : null;
            yield return new SourceRow(startLine, values);
        }
    }

    private static bool OpenQuote(string line)
    {
        var count = 0;
        foreach (var c in line) if (c == '"') count++;
        return count % 2 == 1;
    }

    private static string[] Split(string line)
    {
        var fields = new List<string>();
        var sb = new StringBuilder();
        var quoted = false;
        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (quoted)
            {
                if (c == '"' && i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                else if (c == '"') quoted = false;
                else sb.Append(c);
            }
            else if (c == '"') quoted = true;
            else if (c == ',') { fields.Add(sb.ToString()); sb.Clear(); }
            else sb.Append(c);
        }

        fields.Add(sb.ToString());
        return [.. fields];
    }
}
