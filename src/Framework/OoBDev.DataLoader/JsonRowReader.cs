using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;

namespace OoBDev.DataLoader;

/// <summary>Reads a JSON array of flat objects; scalar values become their text, null stays null.</summary>
public sealed class JsonRowReader : IRowReader
{
    /// <inheritdoc />
    public string Format => "json";

    /// <inheritdoc />
    public async IAsyncEnumerable<SourceRow> ReadAsync(Stream source, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var doc = await JsonDocument.ParseAsync(source, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (doc.RootElement.ValueKind != JsonValueKind.Array) yield break;

        var line = 0;
        foreach (var item in doc.RootElement.EnumerateArray())
        {
            line++;
            if (item.ValueKind != JsonValueKind.Object) continue;
            var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in item.EnumerateObject())
                values[p.Name] = p.Value.ValueKind == JsonValueKind.Null ? null : p.Value.ValueKind == JsonValueKind.String ? p.Value.GetString() : p.Value.GetRawText();
            yield return new SourceRow(line, values);
        }
    }
}
