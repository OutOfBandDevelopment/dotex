using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OoBDev.Data.DesiredState;

/// <summary>The scripts that describe the wanted schema, in file-name order.</summary>
public sealed record DesiredSchema(IReadOnlyList<DesiredScript> Scripts)
{
    /// <summary>The scripts joined into one SQL text.</summary>
    public string Sql => string.Join("\n\n", Scripts.Select(s => s.Text));

    /// <summary>Reads every <c>*.sql</c> file of a folder, ordered by file name.</summary>
    public static DesiredSchema FromDirectory(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        var files = Directory.GetFiles(directory, "*.sql").OrderBy(f => Path.GetFileName(f), StringComparer.Ordinal);
        return new DesiredSchema([.. files.Select(f => new DesiredScript(Path.GetFileName(f), File.ReadAllText(f)))]);
    }
}
