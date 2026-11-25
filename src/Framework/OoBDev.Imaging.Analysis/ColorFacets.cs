using System;
using System.Collections.Generic;
using System.Linq;
using OoBDev.System.Graphics;

namespace OoBDev.Imaging.Analysis;

/// <summary>Colour facets of one image.</summary>
/// <param name="DominantHue">Saturation-weighted circular mean hue in degrees (0 to 360), or null for a gray image.</param>
/// <param name="MeanSaturation">Mean HSV saturation, 0 to 1.</param>
/// <param name="MeanValue">Mean HSV value, 0 to 1.</param>
/// <param name="Palette">Colours by share, largest first.</param>
public sealed record ColorFacets(double? DominantHue, double MeanSaturation, double MeanValue, IReadOnlyList<PaletteColor> Palette)
{
    /// <summary>Measures the image. Cost grows with pixel count, so pass a thumbnail.</summary>
    public static ColorFacets Compute(RgbImage image, int paletteSize = 5)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentOutOfRangeException.ThrowIfLessThan(paletteSize, 1);
        var px = image.Rgb.Span;
        double sat = 0;
        double val = 0;
        double x = 0;
        double y = 0;
        var counts = new SortedDictionary<int, int>();
        for (var i = 0; i < px.Length; i += 3)
        {
            var (hue, s, v) = ColorConversion.Rgb2Hsv(px[i], px[i + 1], px[i + 2]);
            sat += s;
            val += v;
            var radians = hue * Math.PI / 180.0;
            x += s * Math.Cos(radians);
            y += s * Math.Sin(radians);
            var key = (px[i] << 16) | (px[i + 1] << 8) | px[i + 2];
            counts[key] = counts.GetValueOrDefault(key) + 1;
        }

        double n = image.PixelCount;
        double? dominant = Math.Abs(x) < 1e-9 && Math.Abs(y) < 1e-9
            ? null
            : (Math.Atan2(y, x) * 180.0 / Math.PI + 360.0) % 360.0;
        return new ColorFacets(dominant, sat / n, val / n, MedianCut(counts, paletteSize, image.PixelCount));
    }

    private static List<PaletteColor> MedianCut(SortedDictionary<int, int> counts, int size, int total)
    {
        var boxes = new List<List<KeyValuePair<int, int>>> { counts.ToList() };
        while (boxes.Count < size)
        {
            var (index, channel) = WidestBox(boxes);
            if (index < 0)
                break;
            var box = boxes[index];
            var sorted = box.OrderBy(p => Channel(p.Key, channel)).ThenBy(p => p.Key).ToList();
            var half = sorted.Sum(p => p.Value) / 2.0;
            var run = 0;
            var cut = 0;
            for (; cut < sorted.Count - 1; cut++)
            {
                run += sorted[cut].Value;
                if (run >= half)
                    break;
            }

            cut = Math.Min(cut, sorted.Count - 2);
            boxes[index] = sorted.GetRange(0, cut + 1);
            boxes.Add(sorted.GetRange(cut + 1, sorted.Count - cut - 1));
        }

        return [.. boxes
            .Select(b => Average(b, total))
            .OrderByDescending(c => c.Share)
            .ThenBy(c => (c.R << 16) | (c.G << 8) | c.B)];
    }

    private static (int Index, int Channel) WidestBox(List<List<KeyValuePair<int, int>>> boxes)
    {
        var best = (Index: -1, Channel: 0);
        var bestRange = 0;
        for (var i = 0; i < boxes.Count; i++)
        {
            if (boxes[i].Count < 2)
                continue;
            for (var c = 0; c < 3; c++)
            {
                var range = boxes[i].Max(p => Channel(p.Key, c)) - boxes[i].Min(p => Channel(p.Key, c));
                if (range > bestRange)
                {
                    bestRange = range;
                    best = (i, c);
                }
            }
        }

        return best;
    }

    private static int Channel(int key, int channel) => (key >> (16 - (8 * channel))) & 0xFF;

    private static PaletteColor Average(List<KeyValuePair<int, int>> box, int total)
    {
        double count = box.Sum(p => p.Value);
        double r = 0;
        double g = 0;
        double b = 0;
        foreach (var (key, c) in box)
        {
            r += Channel(key, 0) * (double)c;
            g += Channel(key, 1) * (double)c;
            b += Channel(key, 2) * (double)c;
        }

        return new PaletteColor((byte)Math.Round(r / count), (byte)Math.Round(g / count), (byte)Math.Round(b / count), count / total);
    }
}
