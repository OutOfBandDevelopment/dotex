using OoBDev.Documents.ExternalTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace OoBDev.DevTerm.Configuration;

/// <summary>
/// The app-wide list of Stream Monitor converter tools, kept in <see cref="DevTermUserDataPaths.ConverterToolsFile"/>
/// so one registration serves every device and profile. A tool a profile still carries from before the list became
/// global is kept (see <see cref="Merge"/>), so existing profiles keep working.
/// See docs/design/StreamTools/stream-converter-tools.md.
/// </summary>
public sealed class ConverterToolsStore
{
    private static readonly JsonSerializerOptions _json = new() { WriteIndented = true };

    private readonly string _path;

    public ConverterToolsStore(string? path = null) => _path = path ?? DevTermUserDataPaths.ConverterToolsFile;

    /// <summary>The saved tools in order. A missing or unreadable file is an empty list, never an error.</summary>
    public IReadOnlyList<ExternalToolOptions> Load()
    {
        try
        {
            return File.Exists(_path)
                ? [.. (JsonSerializer.Deserialize<List<ExternalToolOptions>>(File.ReadAllText(_path)) ?? []).Select(ExternalToolOptions.Clone)]
                : [];
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return [];
        }
    }

    /// <summary>Replaces the saved list, creating the folder if needed.</summary>
    public void Save(IEnumerable<ExternalToolOptions> tools)
    {
        ArgumentNullException.ThrowIfNull(tools);
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.WriteAllText(_path, JsonSerializer.Serialize(tools.Select(ExternalToolOptions.Clone).ToList(), _json));
    }

    /// <summary>The global tools, then any of <paramref name="profileTools"/> whose name isn't already taken (global wins).</summary>
    public static List<ExternalToolOptions> Merge(IEnumerable<ExternalToolOptions>? globalTools, IEnumerable<ExternalToolOptions>? profileTools)
    {
        var merged = new List<ExternalToolOptions>();
        foreach (var tool in (globalTools ?? []).Concat(profileTools ?? []))
        {
            if (!merged.Any(t => string.Equals(t.Name, tool.Name, StringComparison.OrdinalIgnoreCase)))
            {
                merged.Add(ExternalToolOptions.Clone(tool));
            }
        }

        return merged;
    }
}
