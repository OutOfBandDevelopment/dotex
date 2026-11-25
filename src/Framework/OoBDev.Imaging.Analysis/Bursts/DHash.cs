using System;
using System.Numerics;

namespace OoBDev.Imaging.Analysis.Bursts;

/// <summary>
/// 64-bit difference hash of an 8-bit gray image: a 9x8 Lanczos resize with Pillow's coefficients in 22-bit fixed
/// point, then one bit per left/right neighbour comparison, row-major, as a signed value. The values equal those of
/// the retired Pillow code, so stored hashes stay comparable.
/// </summary>
public static class DHash
{
    private const int PrecisionBits = 22;

    /// <summary>Converts RGBA (4 bytes a pixel) to gray with Pillow's ITU-R 601 weights.</summary>
    public static byte[] GrayFromRgba(ReadOnlySpan<byte> rgba, int width, int height)
    {
        Check(width, height, rgba.Length, 4);
        var gray = new byte[width * height];
        for (var i = 0; i < gray.Length; i++)
            gray[i] = (byte)(((rgba[i * 4] * 19595) + (rgba[(i * 4) + 1] * 38470) + (rgba[(i * 4) + 2] * 7471) + 0x8000) >> 16);
        return gray;
    }

    /// <summary>Converts RGB (3 bytes a pixel) to gray with Pillow's ITU-R 601 weights.</summary>
    public static byte[] GrayFromRgb(ReadOnlySpan<byte> rgb, int width, int height)
    {
        Check(width, height, rgb.Length, 3);
        var gray = new byte[width * height];
        for (var i = 0; i < gray.Length; i++)
            gray[i] = (byte)(((rgb[i * 3] * 19595) + (rgb[(i * 3) + 1] * 38470) + (rgb[(i * 3) + 2] * 7471) + 0x8000) >> 16);
        return gray;
    }

    /// <summary>The hash of a gray image.</summary>
    public static long Compute(byte[] gray, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(gray);
        Check(width, height, gray.Length, 1);
        var small = Resize(Resize(gray, width, height, 9, height, horizontal: true), 9, height, 9, 8, horizontal: false);
        ulong bits = 0;
        for (var row = 0; row < 8; row++)
        {
            for (var col = 0; col < 8; col++)
                bits = (bits << 1) | (small[(row * 9) + col] > small[(row * 9) + col + 1] ? 1UL : 0UL);
        }

        return unchecked((long)bits);
    }

    /// <summary>Number of differing bits between two hashes.</summary>
    public static int Hamming(long a, long b) => BitOperations.PopCount(unchecked((ulong)(a ^ b)));

    internal static byte[] Resize(byte[] src, int w, int h, int ow, int oh, bool horizontal)
    {
        var (bounds, kernels) = Coefficients(horizontal ? w : h, horizontal ? ow : oh);
        var dst = new byte[ow * oh];
        if (horizontal)
        {
            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < ow; x++)
                {
                    var (min, k) = (bounds[x], kernels[x]);
                    long sum = 1L << (PrecisionBits - 1);
                    for (var i = 0; i < k.Length; i++)
                        sum += src[(y * w) + min + i] * (long)k[i];
                    dst[(y * ow) + x] = Clip(sum);
                }
            }
        }
        else
        {
            for (var y = 0; y < oh; y++)
            {
                var (min, k) = (bounds[y], kernels[y]);
                for (var x = 0; x < w; x++)
                {
                    long sum = 1L << (PrecisionBits - 1);
                    for (var i = 0; i < k.Length; i++)
                        sum += src[((min + i) * w) + x] * (long)k[i];
                    dst[(y * ow) + x] = Clip(sum);
                }
            }
        }

        return dst;
    }

    private static void Check(int width, int height, int length, int bytesPerPixel)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        if (length != width * height * bytesPerPixel)
            throw new ArgumentException("The buffer length does not match the size.", nameof(length));
    }

    private static byte Clip(long v) => (byte)Math.Clamp(v >> PrecisionBits, 0, 255);

    private static (int[] Min, int[][] Kernel) Coefficients(int inSize, int outSize)
    {
        var scale = (double)inSize / outSize;
        var filterScale = Math.Max(scale, 1.0);
        var support = 3.0 * filterScale;
        var ss = 1.0 / filterScale;
        var mins = new int[outSize];
        var kernels = new int[outSize][];
        for (var xx = 0; xx < outSize; xx++)
        {
            var center = (xx + 0.5) * scale;
            var xmin = Math.Max(0, (int)(center - support + 0.5));
            var xmax = Math.Min(inSize, (int)(center + support + 0.5)) - xmin;
            var k = new double[xmax];
            double total = 0;
            for (var x = 0; x < xmax; x++)
            {
                k[x] = Lanczos((x + xmin - center + 0.5) * ss);
                total += k[x];
            }

            kernels[xx] = new int[xmax];
            for (var x = 0; x < xmax; x++)
            {
                var norm = total != 0 ? k[x] / total : k[x];
                kernels[xx][x] = (int)(norm < 0 ? -0.5 + (norm * (1 << PrecisionBits)) : 0.5 + (norm * (1 << PrecisionBits)));
            }

            mins[xx] = xmin;
        }

        return (mins, kernels);
    }

    private static double Sinc(double x)
    {
        if (x == 0)
            return 1;
        x *= Math.PI;
        return Math.Sin(x) / x;
    }

    private static double Lanczos(double x) => x is >= -3 and < 3 ? Sinc(x) * Sinc(x / 3) : 0;
}
