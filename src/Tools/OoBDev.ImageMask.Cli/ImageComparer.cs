using SkiaSharp;
using System;

namespace OoBDev.ImageMask.Cli;

/// <summary>Compares two bitmaps pixel by pixel.</summary>
public static class ImageComparer
{
    /// <summary>Counts pixels where any channel differs by more than <paramref name="tolerance"/> (0-255). <paramref name="diff"/> receives a dimmed copy of <paramref name="expected"/> with the differing pixels in magenta, or null when the sizes differ.</summary>
    public static ImageDiffResult Compare(SKBitmap expected, SKBitmap actual, int tolerance, out SKBitmap? diff)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(actual);

        diff = null;
        if (expected.Width != actual.Width || expected.Height != actual.Height)
            return new ImageDiffResult(false, 0, 0);

        diff = new SKBitmap(new SKImageInfo(expected.Width, expected.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul));
        long different = 0;
        for (var y = 0; y < expected.Height; y++)
        {
            for (var x = 0; x < expected.Width; x++)
            {
                var a = expected.GetPixel(x, y);
                var b = actual.GetPixel(x, y);
                var differs = Math.Abs(a.Red - b.Red) > tolerance
                    || Math.Abs(a.Green - b.Green) > tolerance
                    || Math.Abs(a.Blue - b.Blue) > tolerance
                    || Math.Abs(a.Alpha - b.Alpha) > tolerance;
                if (differs)
                    different++;

                diff.SetPixel(x, y, differs
                    ? new SKColor(255, 0, 255)
                    : new SKColor((byte)(a.Red / 3 + 170), (byte)(a.Green / 3 + 170), (byte)(a.Blue / 3 + 170)));
            }
        }

        return new ImageDiffResult(true, different, (long)expected.Width * expected.Height);
    }
}
