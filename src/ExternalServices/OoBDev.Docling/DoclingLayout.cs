using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using OoBDev.Documents.Ocr;

namespace OoBDev.Docling;

/// <summary>Reads text regions from the <c>json_content</c> of a docling document: each item has <c>prov</c> entries with a page and a box, which become fractions of the page size.</summary>
public static class DoclingLayout
{
    private static readonly string[] Collections = ["texts", "tables", "pictures"];

    /// <summary>Extracts the regions in page then top-to-bottom order.</summary>
    /// <param name="document">The <c>json_content</c> object.</param>
    /// <returns>The regions; items without a usable page size or box are skipped.</returns>
    public static IReadOnlyList<OcrRegion> Parse(JsonElement document)
    {
        var sizes = new Dictionary<int, (double W, double H)>();
        if (document.TryGetProperty("pages", out var pages) && pages.ValueKind == JsonValueKind.Object)
        {
            foreach (var page in pages.EnumerateObject())
            {
                if (int.TryParse(page.Name, NumberStyles.None, CultureInfo.InvariantCulture, out var number) && page.Value.TryGetProperty("size", out var size)
                    && Num(size, "width") is { } w && Num(size, "height") is { } h && w > 0 && h > 0)
                {
                    sizes[number] = (w, h);
                }
            }
        }

        var regions = new List<OcrRegion>();
        foreach (var name in Collections)
        {
            if (!document.TryGetProperty(name, out var items) || items.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var item in items.EnumerateArray())
            {
                var label = item.TryGetProperty("label", out var l) ? l.GetString() ?? name : name;
                var text = item.TryGetProperty("text", out var t) ? t.GetString() ?? "" : "";
                if (!item.TryGetProperty("prov", out var prov) || prov.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var p in prov.EnumerateArray())
                {
                    if (Region(p, label, text, sizes) is { } region)
                    {
                        regions.Add(region);
                    }
                }
            }
        }

        regions.Sort(static (a, b) => a.Page != b.Page ? a.Page.CompareTo(b.Page) : a.Top.CompareTo(b.Top));
        return regions;
    }

    private static OcrRegion? Region(JsonElement prov, string label, string text, Dictionary<int, (double W, double H)> sizes)
    {
        if (!prov.TryGetProperty("page_no", out var pn) || !pn.TryGetInt32(out var page) || !sizes.TryGetValue(page, out var size)
            || !prov.TryGetProperty("bbox", out var box)
            || Num(box, "l") is not { } l || Num(box, "r") is not { } r || Num(box, "t") is not { } t || Num(box, "b") is not { } b)
        {
            return null;
        }

        var bottomLeft = box.TryGetProperty("coord_origin", out var o) && string.Equals(o.GetString(), "BOTTOMLEFT", StringComparison.OrdinalIgnoreCase);
        var top = bottomLeft ? size.H - Math.Max(t, b) : Math.Min(t, b);
        var bottom = bottomLeft ? size.H - Math.Min(t, b) : Math.Max(t, b);
        return new OcrRegion(page, label, text, Clamp(Math.Min(l, r) / size.W), Clamp(top / size.H), Clamp(Math.Max(l, r) / size.W), Clamp(bottom / size.H));
    }

    private static double Clamp(double v) => Math.Clamp(v, 0, 1);

    private static double? Num(JsonElement e, string name) => e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetDouble() : null;
}
