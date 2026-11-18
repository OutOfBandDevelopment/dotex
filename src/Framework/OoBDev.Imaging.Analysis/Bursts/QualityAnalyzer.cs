using System;
using System.Numerics;

namespace OoBDev.Imaging.Analysis.Bursts;

/// <summary>
/// Computes the eight <see cref="QualityMeasures"/> from an 8-bit gray buffer (the catalog's Python recipe <c>q1</c> ported to the BCL).
/// The caller supplies the image at the size to measure (the catalog uses a thumbnail with a 1024 long side).
/// Differences from the Python code: borders are left out of the Laplacian and Sobel sums instead of reflected, and the high-frequency
/// share is taken from the largest centred power-of-two square (at most 512) because the FFT here is radix 2.
/// </summary>
public static class QualityAnalyzer
{
    /// <summary>Shortest side that can be measured.</summary>
    public const int MinSide = 32;

    private const int Tiles = 8;
    private const int ClipLow = 3;
    private const int ClipHigh = 252;

    /// <summary>Measures one gray image (row-major, one byte a pixel).</summary>
    /// <exception cref="ArgumentException">The buffer does not match the size or a side is under <see cref="MinSide"/>.</exception>
    public static QualityMeasures Analyze(ReadOnlySpan<byte> gray, int width, int height)
    {
        if (width < MinSide || height < MinSide)
            throw new ArgumentException($"Too small to measure ({width}x{height}).");
        if (gray.Length != checked(width * height))
            throw new ArgumentException("The buffer length does not match the size.", nameof(gray));

        var lap = new double[width * height];
        double sxx = 0, syy = 0, sxy = 0, noiseSum = 0;
        for (var y = 1; y < height - 1; y++)
        {
            for (var x = 1; x < width - 1; x++)
            {
                var i = (y * width) + x;
                double c = gray[i], n = gray[i - width], s = gray[i + width], w = gray[i - 1], e = gray[i + 1];
                double nw = gray[i - width - 1], ne = gray[i - width + 1], sw = gray[i + width - 1], se = gray[i + width + 1];
                lap[i] = n + s + w + e - (4 * c);
                var gx = (ne + (2 * e) + se) - (nw + (2 * w) + sw);
                var gy = (sw + (2 * s) + se) - (nw + (2 * n) + ne);
                sxx += gx * gx;
                syy += gy * gy;
                sxy += gx * gy;
                noiseSum += Math.Abs(nw - (2 * n) + ne - (2 * w) + (4 * c) - (2 * e) + sw - (2 * s) + se);
            }
        }

        var tr = sxx + syy;
        var disc = Math.Sqrt(Math.Max((tr * tr / 4) - ((sxx * syy) - (sxy * sxy)), 0));
        var lMax = (tr / 2) + disc;
        var lMin = Math.Max((tr / 2) - disc, 0);
        var isotropy = lMax > 0 ? lMin / lMax : 1.0;
        var noise = noiseSum * Math.Sqrt(Math.PI / 2) / (6.0 * (width - 2) * (height - 2));

        var histogram = new long[256];
        double sum = 0;
        foreach (var v in gray)
        {
            histogram[v]++;
            sum += v;
        }

        long shadow = 0, highlight = 0;
        for (var v = 0; v < 256; v++)
        {
            if (v <= ClipLow) shadow += histogram[v];
            if (v >= ClipHigh) highlight += histogram[v];
        }

        double total = gray.Length;
        var range = Percentile(histogram, 99) - Percentile(histogram, 1);
        return new QualityMeasures(
            FocusTile: TileFocus(lap, width, height),
            Isotropy: isotropy,
            HfShare: HighFrequencyShare(gray, width, height),
            LumaMean: sum / total,
            ClipShadowPct: 100.0 * shadow / total,
            ClipHighlightPct: 100.0 * highlight / total,
            DynRange: range,
            Noise: noise);
    }

    private static double TileFocus(double[] lap, int width, int height)
    {
        int th = height / Tiles, tw = width / Tiles;
        var variances = new double[Tiles * Tiles];
        for (var ty = 0; ty < Tiles; ty++)
        {
            for (var tx = 0; tx < Tiles; tx++)
            {
                double sum = 0, sumSquares = 0;
                for (var y = ty * th; y < (ty + 1) * th; y++)
                {
                    for (var x = tx * tw; x < (tx + 1) * tw; x++)
                    {
                        var v = lap[(y * width) + x];
                        sum += v;
                        sumSquares += v * v;
                    }
                }

                double n = th * tw;
                var mean = sum / n;
                variances[(ty * Tiles) + tx] = (sumSquares / n) - (mean * mean);
            }
        }

        Array.Sort(variances);
        var position = 0.9 * (variances.Length - 1);
        var lower = (int)Math.Floor(position);
        var upper = Math.Min(lower + 1, variances.Length - 1);
        return variances[lower] + ((position - lower) * (variances[upper] - variances[lower]));
    }

    private static double Percentile(long[] histogram, double percent)
    {
        long total = 0;
        foreach (var count in histogram)
            total += count;
        var position = percent / 100 * (total - 1);
        var lowerRank = (long)Math.Floor(position);
        var fraction = position - lowerRank;
        double lower = ValueAtRank(histogram, lowerRank);
        double upper = ValueAtRank(histogram, Math.Min(lowerRank + 1, total - 1));
        return lower + (fraction * (upper - lower));
    }

    private static int ValueAtRank(long[] histogram, long rank)
    {
        long seen = 0;
        for (var v = 0; v < histogram.Length; v++)
        {
            seen += histogram[v];
            if (rank < seen)
                return v;
        }

        return histogram.Length - 1;
    }

    private static double HighFrequencyShare(ReadOnlySpan<byte> gray, int width, int height)
    {
        var size = 1;
        while (size * 2 <= Math.Min(Math.Min(width, height), 512))
            size *= 2;

        int x0 = (width - size) / 2, y0 = (height - size) / 2;
        var data = new Complex[size * size];
        double mean = 0;
        for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
                mean += gray[((y0 + y) * width) + x0 + x];
        mean /= size * size;
        for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
                data[(y * size) + x] = gray[((y0 + y) * width) + x0 + x] - mean;

        var line = new Complex[size];
        for (var y = 0; y < size; y++)
        {
            Array.Copy(data, y * size, line, 0, size);
            Fft(line);
            Array.Copy(line, 0, data, y * size, size);
        }

        for (var x = 0; x < size; x++)
        {
            for (var y = 0; y < size; y++)
                line[y] = data[(y * size) + x];
            Fft(line);
            for (var y = 0; y < size; y++)
                data[(y * size) + x] = line[y];
        }

        double all = 0, high = 0;
        var half = size / 2.0;
        for (var v = 0; v < size; v++)
        {
            var ry = (((v + (size / 2)) % size) - half) / half;
            for (var u = 0; u < size; u++)
            {
                var rx = (((u + (size / 2)) % size) - half) / half;
                var m = data[(v * size) + u].Magnitude;
                var power = m * m;
                all += power;
                if (Math.Sqrt((rx * rx) + (ry * ry)) > 0.25)
                    high += power;
            }
        }

        return all > 0 ? high / all : 0;
    }

    private static void Fft(Complex[] a)
    {
        var n = a.Length;
        for (int i = 1, j = 0; i < n; i++)
        {
            var bit = n >> 1;
            for (; (j & bit) != 0; bit >>= 1)
                j ^= bit;
            j ^= bit;
            if (i < j)
                (a[i], a[j]) = (a[j], a[i]);
        }

        for (var len = 2; len <= n; len <<= 1)
        {
            var step = Complex.FromPolarCoordinates(1, -2 * Math.PI / len);
            for (var i = 0; i < n; i += len)
            {
                var w = Complex.One;
                for (var k = 0; k < len / 2; k++)
                {
                    var u = a[i + k];
                    var v = a[i + k + (len / 2)] * w;
                    a[i + k] = u + v;
                    a[i + k + (len / 2)] = u - v;
                    w *= step;
                }
            }
        }
    }
}
