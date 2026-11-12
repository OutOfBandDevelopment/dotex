using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OoBDev.Documents.Guides;

namespace OoBDev.Docs.Tests;

/// <summary>
/// Writes one API reference page per project from the XML documentation file its build produced, and removes pages of projects that no longer exist.
/// Deterministic: the same XML files give the same pages.
/// </summary>
public static class ApiReference
{
    private static readonly string[] _skip = ["obj", "node_modules"];

    /// <summary>Writes the pages under <paramref name="folder"/>; returns the page names written or removed.</summary>
    public static IReadOnlyList<string> Write(string root, string folder)
    {
        var changed = new List<string>();
        var pages = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var layer in ProjectCatalog.Layers)
        {
            var layerFolder = Path.Combine(root, "src", layer);
            if (!Directory.Exists(layerFolder))
            {
                continue;
            }

            foreach (var project in Directory.EnumerateFiles(layerFolder, "*.csproj", SearchOption.AllDirectories)
                         .Where(p => !p.Split(Path.DirectorySeparatorChar).Any(s => _skip.Contains(s)))
                         .OrderBy(p => p, StringComparer.Ordinal))
            {
                var name = Path.GetFileNameWithoutExtension(project);
                var xml = Directory.EnumerateFiles(Path.GetDirectoryName(project)!, name + ".xml", SearchOption.AllDirectories)
                    .Where(p => p.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                    .OrderByDescending(File.GetLastWriteTimeUtc)
                    .FirstOrDefault();
                if (xml is not null && !pages.ContainsKey(name + ".md"))
                {
                    pages[name + ".md"] = XmlDocReferenceRenderer.Render(File.ReadAllText(xml));
                }
            }
        }

        Directory.CreateDirectory(folder);
        foreach (var stale in Directory.EnumerateFiles(folder, "*.md").Where(p => !pages.ContainsKey(Path.GetFileName(p))))
        {
            File.Delete(stale);
            changed.Add(Path.GetFileName(stale));
        }

        foreach (var (page, text) in pages)
        {
            if (ApiGuideRenderer.WriteIfChanged(Path.Combine(folder, page), text))
            {
                changed.Add(page);
            }
        }

        return changed;
    }
}
