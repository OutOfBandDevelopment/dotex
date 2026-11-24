using System;
using System.IO;
using Hash = OoBDev.Imaging.Analysis.Bursts.DHash;
using SkiaSharp;

namespace OoBDev.IO.Inventory.Images;

/// <summary>Decodes an image with SkiaSharp and hashes it with the framework dHash (<see cref="OoBDev.Imaging.Analysis.Bursts.DHash"/>), so inventory hashes equal burst-ranking hashes.</summary>
public static class ImageHasher
{
    /// <summary>Hashes an encoded image.</summary>
    /// <param name="image">The encoded image (PNG, JPEG, WebP and the other Skia formats).</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="InvalidDataException">The stream is not a readable image.</exception>
    public static long DHash(Stream image)
    {
        ArgumentNullException.ThrowIfNull(image);
        using var decoded = SKBitmap.Decode(image)
            ?? throw new InvalidDataException("Not a readable image.");
        using var bitmap = decoded.Copy(SKColorType.Rgba8888) ?? throw new InvalidDataException("The image could not be converted.");
        var gray = Hash.GrayFromRgba(bitmap.GetPixelSpan(), bitmap.Width, bitmap.Height);
        return Hash.Compute(gray, bitmap.Width, bitmap.Height);
    }

    /// <summary>The number of differing bits between two hashes.</summary>
    /// <param name="a">One hash.</param>
    /// <param name="b">The other hash.</param>
    /// <returns>0 to 64.</returns>
    public static int Distance(long a, long b) => Hash.Hamming(a, b);
}
