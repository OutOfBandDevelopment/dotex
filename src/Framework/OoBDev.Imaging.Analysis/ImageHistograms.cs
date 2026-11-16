using System;

namespace OoBDev.Imaging.Analysis;

/// <summary>Histograms over the real 0..255 range of 8-bit data.</summary>
public static class ImageHistograms
{
    /// <summary>Counts every pixel; <paramref name="bins"/> (1 to 256) divide the full 0..255 range evenly.</summary>
    public static Histograms Compute(RgbImage image, int bins = 256)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentOutOfRangeException.ThrowIfLessThan(bins, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bins, 256);
        var r = new int[bins];
        var g = new int[bins];
        var b = new int[bins];
        var l = new int[bins];
        var px = image.Rgb.Span;
        for (var i = 0; i < px.Length; i += 3)
        {
            r[px[i] * bins / 256]++;
            g[px[i + 1] * bins / 256]++;
            b[px[i + 2] * bins / 256]++;
            l[RgbImage.Luminance(px[i], px[i + 1], px[i + 2]) * bins / 256]++;
        }

        return new Histograms(r, g, b, l);
    }
}
