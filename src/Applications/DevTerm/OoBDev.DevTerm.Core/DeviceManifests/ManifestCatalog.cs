using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.DeviceManifests;

/// <summary>
/// Lists the manifests a picker offers, from the locations docs/design/ConnectionProfiles/connection-profiles.md
/// describes (the user's <c>~/.dev-term/manifests</c> first, then the ones installed next to the
/// app) — passed in by the caller, since those paths live in <c>DevTerm.Configuration</c>. In each
/// location: every subfolder holding a <c>device.json</c>, every <c>*.zip</c>, and every other
/// <c>*.json</c> file. A folder or JSON file is loaded to read its display name (a zip is listed by
/// file name, so listing never extracts anything); one that fails to load is still listed, so
/// picking it reports why.
/// </summary>
public static class ManifestCatalog
{
    public static IReadOnlyList<ManifestEntry> Discover(params (string Directory, string Source)[] locations)
    {
        ArgumentNullException.ThrowIfNull(locations);

        var entries = new List<ManifestEntry>();
        foreach (var (directory, source) in locations)
        {
            if (!Directory.Exists(directory))
            {
                continue;
            }

            foreach (var folder in Directory.EnumerateDirectories(directory).Order(StringComparer.OrdinalIgnoreCase))
            {
                if (File.Exists(global::System.IO.Path.Combine(folder, DeviceManifestLoader.ManifestFileName)))
                {
                    entries.Add(new ManifestEntry(NameOf(folder, global::System.IO.Path.GetFileName(folder)), folder, source));
                }
            }

            foreach (var file in Directory.EnumerateFiles(directory).Order(StringComparer.OrdinalIgnoreCase))
            {
                var extension = global::System.IO.Path.GetExtension(file);
                if (extension.Equals(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    entries.Add(new ManifestEntry(global::System.IO.Path.GetFileNameWithoutExtension(file), file, source));
                }
                else if (extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
                {
                    entries.Add(new ManifestEntry(NameOf(file, global::System.IO.Path.GetFileNameWithoutExtension(file)), file, source));
                }
            }
        }

        return entries;
    }

    private static string NameOf(string path, string fallback)
    {
        try
        {
            return DeviceManifestLoader.Load(path).Name;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or global::System.Text.Json.JsonException or NotSupportedException)
        {
            return fallback;
        }
    }
}
