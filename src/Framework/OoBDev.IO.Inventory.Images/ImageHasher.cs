using System;
using System.IO;
using System.Numerics;
using SkiaSharp;

namespace OoBDev.IO.Inventory.Images;

/// <summary>A perceptual difference hash: the image is shrunk to 9x8 grey and each pixel is compared with its right neighbour, so resized or recompressed copies hash close together.</summary>
public static class ImageHasher
{
    /// <summary>Hashes an encoded image.</summary>
    /// <param name="image">The encoded image (PNG, JPEG, WebP and the other Skia formats).</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="InvalidDataException">The stream is not a readable image.</exception>
    public static ulong DHash(Stream image)
    {
        ArgumentNullException.ThrowIfNull(image);
        using var source = SKBitmap.Decode(image) ?? throw new InvalidDataException("Not a readable image.");
        using var small = source.Resize(new SKImageInfo(9, 8, SKColorType.Gray8), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear))
            ?? throw new InvalidDataException("The image could not be resized.");

        ulong hash = 0;
        for (var y = 0; y < 8; y++)
        {
            for (var x = 0; x < 8; x++)
            {
                hash <<= 1;
                if (small.GetPixel(x, y).Red > small.GetPixel(x + 1, y).Red) hash |= 1;
            }
        }

        return hash;
    }

    /// <summary>The number of differing bits between two hashes.</summary>
    /// <param name="a">One hash.</param>
    /// <param name="b">The other hash.</param>
    /// <returns>0 to 64.</returns>
    public static int Distance(ulong a, ulong b) => BitOperations.PopCount(a ^ b);
}
