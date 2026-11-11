using System;
using System.Collections.Generic;
using System.IO;

namespace OoBDev.System.IO;

/// <summary>
/// Reads and writes values in an INI file without the Win32 profile API, so it works on every platform.
/// Section and key names are case-insensitive; lines starting with <c>;</c> or <c>#</c> are comments and
/// are kept when a value is written.
/// </summary>
public class IniFile
{
    /// <summary>Creates a reader and writer for <paramref name="iniPath"/>; the file need not exist yet.</summary>
    /// <param name="iniPath">Path of the INI file.</param>
    /// <param name="defaultSection">Section used by the single-key indexer.</param>
    public IniFile(string iniPath, string defaultSection = "Global")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(iniPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultSection);
        Path = global::System.IO.Path.GetFullPath(iniPath);
        DefaultSection = defaultSection;
    }

    /// <summary>The full path of the file.</summary>
    public string Path { get; }

    /// <summary>The section used by the single-key indexer.</summary>
    public string DefaultSection { get; }

    /// <summary>The value of <paramref name="key"/> in the default section; empty when missing.</summary>
    public string this[string key]
    {
        get => Read(DefaultSection, key);
        set => Write(DefaultSection, key, value);
    }

    /// <summary>The value of <paramref name="key"/> in <paramref name="section"/>; empty when missing.</summary>
    public string this[string section, string key]
    {
        get => Read(section, key);
        set => Write(section, key, value);
    }

    /// <summary>Reads a value, returning <paramref name="defaultValue"/> when the file, section or key is missing.</summary>
    public string Read(string section, string key, string defaultValue = "")
    {
        if (!File.Exists(Path))
        {
            return defaultValue;
        }

        var inSection = false;
        foreach (var line in File.ReadLines(Path))
        {
            if (TryGetSection(line, out var name))
            {
                inSection = Same(name, section);
            }
            else if (inSection && TryGetPair(line, out var k, out var v) && Same(k, key))
            {
                return v;
            }
        }
        return defaultValue;
    }

    /// <summary>Writes a value, creating the file, section or key as needed and keeping every other line.</summary>
    public void Write(string section, string key, string value)
    {
        var lines = File.Exists(Path) ? new List<string>(File.ReadAllLines(Path)) : [];
        var entry = $"{key}={value}";
        var sectionStart = -1;
        var insertAt = -1;

        for (var i = 0; i < lines.Count; i++)
        {
            if (TryGetSection(lines[i], out var name))
            {
                if (sectionStart >= 0)
                {
                    break;
                }
                if (Same(name, section))
                {
                    sectionStart = i;
                    insertAt = i + 1;
                }
            }
            else if (sectionStart >= 0)
            {
                if (TryGetPair(lines[i], out var k, out _) && Same(k, key))
                {
                    lines[i] = entry;
                    File.WriteAllLines(Path, lines);
                    return;
                }
                if (!string.IsNullOrWhiteSpace(lines[i]))
                {
                    insertAt = i + 1;
                }
            }
        }

        if (sectionStart < 0)
        {
            if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines[^1]))
            {
                lines.Add(string.Empty);
            }
            lines.Add($"[{section}]");
            lines.Add(entry);
        }
        else
        {
            lines.Insert(insertAt, entry);
        }
        File.WriteAllLines(Path, lines);
    }

    private static bool Same(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    private static bool TryGetSection(string line, out string name)
    {
        var text = line.Trim();
        if (text.Length > 2 && text[0] == '[' && text[^1] == ']')
        {
            name = text[1..^1].Trim();
            return true;
        }
        name = string.Empty;
        return false;
    }

    private static bool TryGetPair(string line, out string key, out string value)
    {
        var text = line.Trim();
        var split = text.IndexOf('=', StringComparison.Ordinal);
        if (text.Length == 0 || text[0] is ';' or '#' or '[' || split <= 0)
        {
            key = value = string.Empty;
            return false;
        }
        key = text[..split].Trim();
        value = text[(split + 1)..].Trim();
        return true;
    }
}
