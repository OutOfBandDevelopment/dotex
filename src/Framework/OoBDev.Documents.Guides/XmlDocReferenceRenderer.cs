using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace OoBDev.Documents.Guides;

/// <summary>Renders an API reference (markdown) from a compiler XML documentation file, deterministically: same file, same bytes.</summary>
public static class XmlDocReferenceRenderer
{
    private static readonly (char Kind, string Heading)[] _memberKinds =
    [
        ('F', "Fields"),
        ('P', "Properties"),
        ('M', "Methods and constructors"),
        ('E', "Events"),
    ];

    /// <summary>One section per documented type (ordinal order) with its documented members; members that only inherit their documentation are left out.</summary>
    public static string Render(string xml)
    {
        var document = XDocument.Parse(xml);
        var assembly = document.Root?.Element("assembly")?.Element("name")?.Value ?? "Assembly";
        var members = (document.Root?.Element("members")?.Elements("member") ?? [])
            .Where(m => m.Attribute("name")?.Value is { Length: > 2 } && !m.Descendants("inheritdoc").Any())
            .Select(m => (Name: m.Attribute("name")!.Value, Element: m))
            .ToList();

        var types = members
            .Where(m => m.Name[0] == 'T')
            .OrderBy(m => m.Name, StringComparer.Ordinal)
            .ToList();
        var typeNames = types.Select(t => t.Name[2..]).ToHashSet(StringComparer.Ordinal);
        var byOwner = members
            .Where(m => m.Name[0] != 'T' && m.Name[0] != 'N')
            .Select(m => (Owner: OwnerOf(m.Name, typeNames), m.Name, m.Element))
            .Where(m => m.Owner is not null)
            .ToLookup(m => m.Owner!, StringComparer.Ordinal);

        var text = new StringBuilder();
        text.Append("# ").Append(assembly).Append("\n\n");
        text.Append("Generated from the compiler XML documentation; do not edit by hand.\n\n");
        if (types.Count == 0)
        {
            text.Append("No documented types.\n");
            return text.ToString();
        }

        foreach (var type in types)
        {
            var fullName = type.Name[2..];
            text.Append("## ").Append(Inline(fullName)).Append("\n\n");
            AppendParagraph(text, type.Element.Element("summary"));
            AppendParagraph(text, type.Element.Element("remarks"));
            foreach (var (kind, heading) in _memberKinds)
            {
                var rows = byOwner[fullName]
                    .Where(m => m.Name[0] == kind)
                    .OrderBy(m => m.Name, StringComparer.Ordinal)
                    .ToList();
                if (rows.Count == 0)
                {
                    continue;
                }

                text.Append("### ").Append(heading).Append("\n\n");
                foreach (var row in rows)
                {
                    text.Append("- `").Append(MemberLabel(row.Name, fullName)).Append('`');
                    var summary = Flatten(row.Element.Element("summary"));
                    if (summary.Length > 0)
                    {
                        text.Append(": ").Append(summary);
                    }

                    text.Append('\n');
                }

                text.Append('\n');
            }
        }

        return text.ToString();
    }

    private static string? OwnerOf(string memberName, HashSet<string> typeNames)
    {
        var name = memberName[2..];
        var parameters = name.IndexOf('(', StringComparison.Ordinal);
        if (parameters >= 0)
        {
            name = name[..parameters];
        }

        for (var dot = name.LastIndexOf('.'); dot > 0; dot = name.LastIndexOf('.'))
        {
            name = name[..dot];
            if (typeNames.Contains(name))
            {
                return name;
            }
        }

        return null;
    }

    private static string MemberLabel(string memberName, string owner)
    {
        var name = memberName[(2 + owner.Length + 1)..];
        return name.Replace("#ctor", "constructor", StringComparison.Ordinal);
    }

    private static void AppendParagraph(StringBuilder text, XElement? element)
    {
        var flat = Flatten(element);
        if (flat.Length > 0)
        {
            text.Append(flat).Append("\n\n");
        }
    }

    private static string Flatten(XElement? element)
    {
        if (element is null)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var node in element.Nodes())
        {
            switch (node)
            {
                case XText t:
                    builder.Append(t.Value);
                    break;
                case XElement e when e.Name.LocalName is "c" or "code":
                    builder.Append('`').Append(Collapse(e.Value)).Append('`');
                    break;
                case XElement e when e.Name.LocalName is "see" or "seealso":
                    builder.Append('`').Append(LastSegment(e.Attribute("cref")?.Value ?? e.Attribute("langword")?.Value ?? e.Value)).Append('`');
                    break;
                case XElement e when e.Name.LocalName is "paramref" or "typeparamref":
                    builder.Append('`').Append(e.Attribute("name")?.Value).Append('`');
                    break;
                case XElement e:
                    builder.Append(' ').Append(Flatten(e)).Append(' ');
                    break;
            }
        }

        return Collapse(builder.ToString()).Replace("|", "\\|", StringComparison.Ordinal);
    }

    private static string LastSegment(string cref)
    {
        var name = cref.Length > 1 && cref[1] == ':' ? cref[2..] : cref;
        var parameters = name.IndexOf('(', StringComparison.Ordinal);
        if (parameters >= 0)
        {
            name = name[..parameters];
        }

        return name[(name.LastIndexOf('.') + 1)..];
    }

    private static string Collapse(string value) =>
        string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static string Inline(string value) => value.Replace("`", "\\`", StringComparison.Ordinal);
}
