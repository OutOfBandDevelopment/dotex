using System;
using System.IO;
using SkiaSharp;

namespace OoBDev.Extensions.Drawing;

/// <summary>Stores arbitrary bytes as the pixels of a lossless PNG image, and reads them back.</summary>
public static class PngPack
{
    private const int HeaderLength = sizeof(int);

    /// <summary>Packs bytes into a roughly square PNG; the first four bytes of pixel data hold the length.</summary>
    /// <param name="input">The bytes to store.</param>
    /// <returns>The PNG file.</returns>
    public static byte[] Pack(byte[] input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var pixels = (input.Length + HeaderLength + 3) / 4;
        var width = (int)Math.Ceiling(Math.Sqrt(pixels));
        var height = (int)Math.Ceiling(pixels / (double)width);

        // Unpremul keeps every byte as written, including colour bytes of transparent pixels.
        using var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Unpremul));
        var span = bitmap.GetPixelSpan();
        BitConverter.TryWriteBytes(span, input.Length);
        input.CopyTo(span[HeaderLength..]);

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    /// <summary>Reads back the bytes stored by <see cref="Pack"/>.</summary>
    /// <param name="input">A PNG file made by <see cref="Pack"/>.</param>
    /// <exception cref="InvalidDataException">The stored length does not fit the image.</exception>
    public static byte[] Unpack(byte[] input)
    {
        ArgumentNullException.ThrowIfNull(input);

        using var codec = SKCodec.Create(new MemoryStream(input))
            ?? throw new InvalidDataException("The data is not an image.");
        var info = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
        using var bitmap = new SKBitmap(info);
        if (codec.GetPixels(info, bitmap.GetPixels()) is not (SKCodecResult.Success or SKCodecResult.IncompleteInput))
            throw new InvalidDataException("The image could not be decoded.");

        var span = bitmap.GetPixelSpan();
        var length = BitConverter.ToInt32(span[..HeaderLength]);
        var capacity = (long)info.Width * info.Height * 4 - HeaderLength;
        if (length < 0 || length > capacity)
            throw new InvalidDataException("The image does not hold packed data.");

        return span.Slice(HeaderLength, length).ToArray();
    }
}
