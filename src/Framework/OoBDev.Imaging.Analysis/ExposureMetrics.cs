using System;

namespace OoBDev.Imaging.Analysis;

/// <summary>Exposure numbers of one image.</summary>
/// <param name="MeanLuminance">Mean luminance, 0 to 255.</param>
/// <param name="ClippedBlackPct">Percent of pixels with luminance at or below the black threshold.</param>
/// <param name="ClippedWhitePct">Percent of pixels with luminance at or above the white threshold.</param>
/// <param name="Contrast">Standard deviation of luminance.</param>
public sealed record ExposureMetrics(double MeanLuminance, double ClippedBlackPct, double ClippedWhitePct, double Contrast)
{
    /// <summary>Measures the image; a pixel is clipped at or beyond the thresholds (defaults 0 and 255).</summary>
    public static ExposureMetrics Compute(RgbImage image, byte blackThreshold = 0, byte whiteThreshold = 255)
    {
        ArgumentNullException.ThrowIfNull(image);
        var px = image.Rgb.Span;
        double sum = 0;
        double sumSquares = 0;
        long black = 0;
        long white = 0;
        for (var i = 0; i < px.Length; i += 3)
        {
            var y = RgbImage.Luminance(px[i], px[i + 1], px[i + 2]);
            sum += y;
            sumSquares += (double)y * y;
            if (y <= blackThreshold)
                black++;
            if (y >= whiteThreshold)
                white++;
        }

        double n = image.PixelCount;
        var mean = sum / n;
        var variance = Math.Max(0, (sumSquares / n) - (mean * mean));
        return new ExposureMetrics(mean, 100.0 * black / n, 100.0 * white / n, Math.Sqrt(variance));
    }
}
