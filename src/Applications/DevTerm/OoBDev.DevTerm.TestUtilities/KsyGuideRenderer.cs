using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using OoBDev.DevTerm.DeviceManifests;
using YamlDotNet.Serialization;

namespace OoBDev.DevTerm.TestUtilities;

/// <summary>Renders a binary-format guide (markdown) from Kaitai Struct <c>.ksy</c> files: one section per file with its title, description and field table, in file-name order.</summary>
public static class KsyGuideRenderer
{
    /// <summary>The guide for <paramref name="files"/> (file name, <c>.ksy</c> text); a file that does not import is listed with its warnings.</summary>
    public static string Render(string title, IEnumerable<(string FileName, string Ksy)> files)
    {
        var text = new StringBuilder();
        text.Append("# ").Append(title).Append("\n\n");
        text.Append("Generated from the `.ksy` files next to each device library by `KsyGuideTests`; do not edit by hand. Sizes and offsets come from the DevTerm importer ([ksy importer](../../../design/UiDefinitions/ksy-importer.md)).\n\n");
        var ordered = files.OrderBy(f => f.FileName, StringComparer.Ordinal).ToList();
        text.Append("## Formats\n\n");
        var table = 0;
        foreach (var (fileName, ksy) in ordered)
        {
            var (id, formatTitle, doc) = Meta(ksy);
            text.Append("### ").Append(id.Length > 0 ? id : fileName).Append("\n\n");
            text.Append("File: `").Append(fileName).Append("`\n\n");
            if (formatTitle.Length > 0)
            {
                text.Append(formatTitle).Append("\n\n");
            }

            if (doc.Length > 0)
            {
                text.Append(doc).Append("\n\n");
            }

            var result = KsyImporter.Import(ksy);
            foreach (var warning in result.Warnings)
            {
                text.Append("- Import warning: ").Append(warning.ReplaceLineEndings(" ")).Append('\n');
            }

            if (result.Warnings.Count > 0)
            {
                text.Append('\n');
            }

            if (result.Schema is not { } schema)
            {
                continue;
            }

            var length = schema.Length is { } fixedLength ? fixedLength.ToString(CultureInfo.InvariantCulture) + " bytes" : schema.MinLength is { } min ? "at least " + min.ToString(CultureInfo.InvariantCulture) + " bytes" : "variable";
            table++;
            text.Append("**Table ").Append(table.ToString(CultureInfo.InvariantCulture)).Append(" — Fields of ").Append(id.Length > 0 ? id : fileName).Append(" (").Append(length).Append(", ").Append(schema.Endian == "be" ? "big-endian" : "little-endian").Append(")**\n\n");
            text.Append("| Field | Type | Size | Notes |\n|-------|------|------|-------|\n");
            foreach (var field in schema.Fields)
            {
                var size = field.EffectiveSize?.ToString(CultureInfo.InvariantCulture) ?? (field.BitWidth > 0 ? field.BitWidth.ToString(CultureInfo.InvariantCulture) + " bits" : string.Empty);
                var notes = string.Join("; ", new[] { field.Label, field.Expect is { Length: > 0 } expect ? "expects " + expect : null }.Where(n => !string.IsNullOrWhiteSpace(n)));
                text.Append("| `").Append(field.Name).Append("` | ").Append(field.Type).Append(" | ").Append(size).Append(" | ").Append(Cell(notes)).Append(" |\n");
            }

            text.Append('\n');
        }

        return text.ToString();
    }

    private static string Cell(string value) => value.ReplaceLineEndings(" ").Replace("|", "\\|", StringComparison.Ordinal);

    private static (string Id, string Title, string Doc) Meta(string ksy)
    {
        try
        {
            if (new DeserializerBuilder().Build().Deserialize<object>(ksy) is not Dictionary<object, object> root)
            {
                return (string.Empty, string.Empty, string.Empty);
            }

            var meta = root.TryGetValue("meta", out var m) ? m as Dictionary<object, object> : null;
            string Get(Dictionary<object, object>? map, string key) => map is not null && map.TryGetValue(key, out var v) && v is string s ? s.Trim().ReplaceLineEndings(" ") : string.Empty;
            return (Get(meta, "id"), Get(meta, "title"), Get(root, "doc"));
        }
        catch (YamlDotNet.Core.YamlException)
        {
            return (string.Empty, string.Empty, string.Empty);
        }
    }
}
