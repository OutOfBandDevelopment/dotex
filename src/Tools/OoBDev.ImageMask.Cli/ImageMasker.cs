using SkiaSharp;
using System;
using System.IO;
using System.IO.Enumeration;

namespace OoBDev.ImageMask.Cli;

/// <summary>Paints solid blocks over the zones of an image so volatile regions compare equal.</summary>
public static class ImageMasker
{
    /// <summary>Paints every zone of <paramref name="definition"/> whose <see cref="MaskZone.Match"/> fits <paramref name="fileName"/>; zones partly outside the image are clipped.</summary>
    public static void Apply(SKBitmap bitmap, MaskDefinition definition, string fileName)
    {
        ArgumentNullException.ThrowIfNull(bitmap);
        ArgumentNullException.ThrowIfNull(definition);

        using var canvas = new SKCanvas(bitmap);
        foreach (var zone in definition.Zones)
        {
            if (!FileSystemName.MatchesSimpleExpression(zone.Match, fileName, ignoreCase: true))
                continue;

            if (!SKColor.TryParse(zone.Color, out var color))
                throw new FormatException($"Zone '{zone.Name}' has an invalid color '{zone.Color}'.");

            using var paint = new SKPaint { Color = color, Style = SKPaintStyle.Fill, IsAntialias = false };
            canvas.DrawRect(SKRect.Create(zone.X, zone.Y, zone.Width, zone.Height), paint);
        }
    }

    /// <summary>Loads an image file as unpremultiplied RGBA.</summary>
    public static SKBitmap Load(string path)
    {
        using var data = SKData.Create(path) ?? throw new FileNotFoundException("Image not found or unreadable.", path);
        using var decoded = SKBitmap.Decode(data) ?? throw new InvalidDataException($"'{path}' is not a supported image.");

        var copy = new SKBitmap(new SKImageInfo(decoded.Width, decoded.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul));
        decoded.CopyTo(copy, SKColorType.Rgba8888);
        return copy;
    }

    /// <summary>Saves <paramref name="bitmap"/> as PNG.</summary>
    public static void SavePng(SKBitmap bitmap, string path)
    {
        ArgumentNullException.ThrowIfNull(bitmap);

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.Create(path);
        data.SaveTo(stream);
    }
}
