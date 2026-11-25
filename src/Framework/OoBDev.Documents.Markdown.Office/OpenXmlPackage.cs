using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace OoBDev.Documents.Markdown.Office;

/// <summary>Reads the parts of an Open XML (zip) package without an office library.</summary>
internal sealed class OpenXmlPackage : IDisposable
{
    private static readonly XNamespace _relationships = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private static readonly XNamespace _packageRelationships = "http://schemas.openxmlformats.org/package/2006/relationships";

    private readonly ZipArchive _archive;

    public OpenXmlPackage(string path)
    {
        _archive = ZipFile.OpenRead(path);
    }

    public static XNamespace RelationshipNamespace => _relationships;

    public XDocument? Load(string partName)
    {
        var entry = _archive.GetEntry(partName.TrimStart('/'));
        if (entry is null)
        {
            return null;
        }

        using var stream = entry.Open();
        return XDocument.Load(stream);
    }

    /// <summary>The relationship targets of a part by relationship id, resolved to package paths.</summary>
    public Dictionary<string, string> Relationships(string partName)
    {
        var directory = partName.Contains('/') ? partName[..partName.LastIndexOf('/')] : "";
        var file = partName[(partName.LastIndexOf('/') + 1)..];
        var relsPart = (directory.Length == 0 ? "" : directory + "/") + "_rels/" + file + ".rels";
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        var doc = Load(relsPart);
        if (doc is null)
        {
            return result;
        }

        foreach (var rel in doc.Descendants(_packageRelationships + "Relationship"))
        {
            var id = (string?)rel.Attribute("Id");
            var target = (string?)rel.Attribute("Target");
            if (id is null || target is null)
            {
                continue;
            }

            result[id] = target.StartsWith('/') ? target.TrimStart('/') : Resolve(directory, target);
        }

        return result;
    }

    private static string Resolve(string directory, string target)
    {
        var parts = new List<string>(directory.Split('/', StringSplitOptions.RemoveEmptyEntries));
        foreach (var segment in target.Split('/'))
        {
            if (segment == "..")
            {
                if (parts.Count > 0)
                {
                    parts.RemoveAt(parts.Count - 1);
                }
            }
            else if (segment != ".")
            {
                parts.Add(segment);
            }
        }

        return string.Join("/", parts);
    }

    public void Dispose() => _archive.Dispose();
}
