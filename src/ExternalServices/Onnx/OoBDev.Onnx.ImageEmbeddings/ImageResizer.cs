using System;

namespace OoBDev.Onnx.ImageEmbeddings;

/// <summary>Resampling filter, numbered like Pillow's <c>Image.Resampling</c>.</summary>
public enum ImageResample
{
    /// <summary>Triangle filter (Pillow <c>BILINEAR</c>, value 2).</summary>
    Bilinear = 2,

    /// <summary>Cubic filter with a = -0.5 (Pillow <c>BICUBIC</c>, value 3).</summary>
    Bicubic = 3,
}

/// <summary>
/// Antialiased resize that reproduces Pillow's 8-bit separable convolution (filter support grows with the reduction
/// ratio, 22 bit fixed point coefficients, horizontal pass first), because the Python image processors the models
/// were evaluated with use it and decoder libraries resize differently.
/// </summary>
public static class ImageResizer
{
    private const int PrecisionBits = 32 - 8 - 2;

    /// <summary>
    /// Resizes interleaved 8-bit RGB pixels.
    /// </summary>
    /// <param name="rgb">Source pixels, <c>width * height * 3</c> bytes.</param>
    /// <param name="width">Source width.</param>
    /// <param name="height">Source height.</param>
    /// <param name="newWidth">Target width.</param>
    /// <param name="newHeight">Target height.</param>
    /// <param name="filter">Resampling filter.</param>
    /// <returns>The resized pixels (a copy, also when the size is unchanged).</returns>
    public static byte[] Resize(byte[] rgb, int width, int height, int newWidth, int newHeight, ImageResample filter)
    {
        ArgumentNullException.ThrowIfNull(rgb);
        if (width < 1 || height < 1 || newWidth < 1 || newHeight < 1) throw new ArgumentOutOfRangeException(nameof(width), "Sizes must be positive.");
        if (rgb.Length != checked(width * height * 3)) throw new ArgumentException("Pixel buffer does not match the size.", nameof(rgb));

        var current = rgb;
        var currentWidth = width;
        if (newWidth != width)
        {
            current = Horizontal(current, currentWidth, height, newWidth, filter);
            currentWidth = newWidth;
        }
        if (newHeight != height) current = Vertical(current, currentWidth, height, newHeight, filter);
        return ReferenceEquals(current, rgb) ? (byte[])rgb.Clone() : current;
    }

    private static byte[] Horizontal(byte[] source, int width, int height, int newWidth, ImageResample filter)
    {
        var (bounds, coefficients, kernel) = Coefficients(width, newWidth, filter);
        var target = new byte[newWidth * height * 3];
        for (var y = 0; y < height; y++)
        {
            var row = y * width * 3;
            var outRow = y * newWidth * 3;
            for (var x = 0; x < newWidth; x++)
            {
                var (start, count) = bounds[x];
                var k = x * kernel;
                for (var c = 0; c < 3; c++)
                {
                    var sum = 1 << (PrecisionBits - 1);
                    for (var i = 0; i < count; i++) sum += source[row + ((start + i) * 3) + c] * coefficients[k + i];
                    target[outRow + (x * 3) + c] = Clip(sum);
                }
            }
        }
        return target;
    }

    private static byte[] Vertical(byte[] source, int width, int height, int newHeight, ImageResample filter)
    {
        var (bounds, coefficients, kernel) = Coefficients(height, newHeight, filter);
        var target = new byte[width * newHeight * 3];
        var stride = width * 3;
        for (var y = 0; y < newHeight; y++)
        {
            var (start, count) = bounds[y];
            var k = y * kernel;
            for (var i3 = 0; i3 < stride; i3++)
            {
                var sum = 1 << (PrecisionBits - 1);
                for (var i = 0; i < count; i++) sum += source[((start + i) * stride) + i3] * coefficients[k + i];
                target[(y * stride) + i3] = Clip(sum);
            }
        }
        return target;
    }

    private static byte Clip(int sum)
    {
        var value = sum >> PrecisionBits;
        return value < 0 ? (byte)0 : value > 255 ? (byte)255 : (byte)value;
    }

    private static ((int Start, int Count)[] Bounds, int[] Coefficients, int Kernel) Coefficients(int inSize, int outSize, ImageResample filter)
    {
        var scale = (double)inSize / outSize;
        var filterScale = Math.Max(scale, 1.0);
        var support = (filter == ImageResample.Bicubic ? 2.0 : 1.0) * filterScale;
        var kernel = (int)Math.Ceiling(support) * 2 + 1;
        var ss = 1.0 / filterScale;

        var bounds = new (int, int)[outSize];
        var coefficients = new int[outSize * kernel];
        var weights = new double[kernel];
        for (var xx = 0; xx < outSize; xx++)
        {
            var center = (xx + 0.5) * scale;
            var xmin = Math.Max((int)(center - support + 0.5), 0);
            var xmax = Math.Min((int)(center + support + 0.5), inSize) - xmin;
            var total = 0.0;
            for (var x = 0; x < xmax; x++)
            {
                var w = Weight(filter, (x + xmin - center + 0.5) * ss);
                weights[x] = w;
                total += w;
            }
            for (var x = 0; x < xmax; x++)
            {
                var w = total != 0 ? weights[x] / total : weights[x];
                coefficients[(xx * kernel) + x] = (int)(w < 0 ? -0.5 + (w * (1 << PrecisionBits)) : 0.5 + (w * (1 << PrecisionBits)));
            }
            bounds[xx] = (xmin, xmax);
        }
        return (bounds, coefficients, kernel);
    }

    private static double Weight(ImageResample filter, double x)
    {
        x = Math.Abs(x);
        if (filter == ImageResample.Bilinear) return x < 1.0 ? 1.0 - x : 0.0;
        const double a = -0.5;
        if (x < 1.0) return ((((a + 2.0) * x) - (a + 3.0)) * x * x) + 1.0;
        if (x < 2.0) return ((((x - 5.0) * x) + 8.0) * x - 4.0) * a;
        return 0.0;
    }
}
