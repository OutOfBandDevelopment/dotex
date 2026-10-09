using SkiaSharp;
using System;
using System.IO;

namespace OoBDev.Onnx.ImageEmbeddings.Skia;

/// <summary>
/// <see cref="IImageDecoder"/> built on SkiaSharp (JPEG, PNG, WebP, GIF, BMP, ICO, ...).
/// </summary>
public sealed class SkiaImageDecoder : IImageDecoder
{
    /// <inheritdoc />
    public DecodedImage Decode(ReadOnlySpan<byte> encoded, long maxPixels)
    {
        using var data = SKData.CreateCopy(encoded);
        using var codec = SKCodec.Create(data) ?? throw new InvalidDataException("The bytes are not a supported image.");
        var info = codec.Info;
        if (info.Width < 1 || info.Height < 1) throw new InvalidDataException("The image has no pixels.");
        if ((long)info.Width * info.Height > maxPixels)
            throw new InvalidDataException($"The image is {info.Width}x{info.Height}, larger than the allowed {maxPixels} pixels.");

        // Unpremultiplied so the stored colour is kept when alpha is dropped, like Pillow's convert("RGB").
        var target = new SKImageInfo(info.Width, info.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
        var rgba = new byte[target.Width * target.Height * 4];
        var result = codec.GetPixels(target, rgba);
        if (result is not (SKCodecResult.Success or SKCodecResult.IncompleteInput))
            throw new InvalidDataException($"The image could not be decoded ({result}).");

        var rgb = new byte[target.Width * target.Height * 3];
        for (int s = 0, d = 0; s < rgba.Length; s += 4, d += 3)
        {
            rgb[d] = rgba[s];
            rgb[d + 1] = rgba[s + 1];
            rgb[d + 2] = rgba[s + 2];
        }
        return new DecodedImage(target.Width, target.Height, rgb);
    }
}
