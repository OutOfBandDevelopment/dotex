using System;
using System.Drawing;
using System.IO;
using SkiaSharp;

namespace OoBDev.Extensions.Drawing;

/// <summary>
/// Cuts an image into a pyramid of square tiles, as used by deep-zoom viewers. Level <c>n</c> shows the whole image
/// at scale <c>2^n / 2^max</c>; the top level (<see cref="GetMaxLevel(SKBitmap)"/>) is full size.
/// </summary>
public static class MultiScaleTile
{
    /// <summary>The usual tile edge length in pixels.</summary>
    public const int DefaultTileSize = 256;

    private const int JpegQuality = 75;

    private static readonly SKSamplingOptions _sampling = new(SKFilterMode.Linear, SKMipmapMode.Linear);

    private static Size SizeOf(SKBitmap image) => new(image.Width, image.Height);

    /// <summary>Gets the top level of the pyramid for the image.</summary>
    public static int GetMaxLevel(this SKBitmap image)
    {
        ArgumentNullException.ThrowIfNull(image);
        return SizeOf(image).MaxLevel();
    }

    /// <summary>Gets how many tiles wide and high a level is.</summary>
    public static Size GetTileCount(this SKBitmap image, int level, int tileSize = DefaultTileSize)
    {
        ArgumentNullException.ThrowIfNull(image);
        var factor = Math.Pow(2, level - SizeOf(image).MaxLevel());
        return new Size(
            (int)Math.Ceiling(image.Width * factor / tileSize),
            (int)Math.Ceiling(image.Height * factor / tileSize));
    }

    /// <summary>Renders one tile; the caller disposes it.</summary>
    /// <param name="image">The source image.</param>
    /// <param name="level">The pyramid level.</param>
    /// <param name="x">The tile column.</param>
    /// <param name="y">The tile row.</param>
    /// <param name="tileSize">The tile edge length in pixels.</param>
    public static SKBitmap GetTile(this SKBitmap image, int level, int x, int y, int tileSize = DefaultTileSize)
    {
        ArgumentNullException.ThrowIfNull(image);

        var size = SizeOf(image);
        var maxLevelLength = Math.Pow(2, size.MaxLevel());
        var levelLength = Math.Pow(2, level);
        var levelScale = tileSize / levelLength;

        if (levelScale >= 1)
        {
            var scaled = size.Scale(levelLength / maxLevelLength);
            return image.Resize(new SKImageInfo(scaled.Width, scaled.Height, image.ColorType, image.AlphaType), _sampling)
                ?? throw new InvalidOperationException("The image could not be scaled.");
        }

        var maxLevelSize = new Size((int)maxLevelLength, (int)maxLevelLength);
        var extractSize = maxLevelSize.Scale(levelScale);
        var origin = new Point(x, y).OffsetBy(extractSize);
        var source = SKRect.Create(origin.X, origin.Y, extractSize.Width, extractSize.Height);

        var tile = new SKBitmap(tileSize, tileSize);
        try
        {
            using var canvas = new SKCanvas(tile);
            using var picture = SKImage.FromBitmap(image);
            canvas.DrawImage(picture, source, SKRect.Create(0, 0, tileSize, tileSize), _sampling);
        }
        catch
        {
            tile.Dispose();
            throw;
        }

        return tile;
    }

    /// <summary>Renders one tile and encodes it as JPEG.</summary>
    public static byte[] GetTileAsBytes(this SKBitmap image, int level, int x, int y, int tileSize = DefaultTileSize)
    {
        using var tile = image.GetTile(level, x, y, tileSize);
        using var picture = SKImage.FromBitmap(tile);
        using var data = picture.Encode(SKEncodedImageFormat.Jpeg, JpegQuality);
        return data.ToArray();
    }

    /// <summary>Renders one tile as a JPEG stream positioned at the start; the caller disposes it.</summary>
    public static Stream GetTileAsStream(this SKBitmap image, int level, int x, int y, int tileSize = DefaultTileSize) =>
        new MemoryStream(image.GetTileAsBytes(level, x, y, tileSize), writable: false);

    /// <summary>Gets the top level of the pyramid for the image file.</summary>
    public static int GetMaxLevel(this string imagePath)
    {
        using var image = DecodeFile(imagePath);
        return image.GetMaxLevel();
    }

    /// <summary>Gets how many tiles wide and high a level is for the image file.</summary>
    public static Size GetTileCount(this string imagePath, int level, int tileSize = DefaultTileSize)
    {
        using var image = DecodeFile(imagePath);
        return image.GetTileCount(level, tileSize);
    }

    /// <summary>Renders one tile of the image file and encodes it as JPEG.</summary>
    public static byte[] GetTileAsBytes(this string imagePath, int level, int x, int y, int tileSize = DefaultTileSize)
    {
        using var image = DecodeFile(imagePath);
        return image.GetTileAsBytes(level, x, y, tileSize);
    }

    /// <summary>Gets the top level of the pyramid for the image in a stream.</summary>
    public static int GetMaxLevel(this Stream imageStream)
    {
        using var image = DecodeStream(imageStream);
        return image.GetMaxLevel();
    }

    /// <summary>Gets how many tiles wide and high a level is for the image in a stream.</summary>
    public static Size GetTileCount(this Stream imageStream, int level, int tileSize = DefaultTileSize)
    {
        using var image = DecodeStream(imageStream);
        return image.GetTileCount(level, tileSize);
    }

    /// <summary>Renders one tile of the image in a stream and encodes it as JPEG.</summary>
    public static byte[] GetTileAsBytes(this Stream imageStream, int level, int x, int y, int tileSize = DefaultTileSize)
    {
        using var image = DecodeStream(imageStream);
        return image.GetTileAsBytes(level, x, y, tileSize);
    }

    private static SKBitmap DecodeFile(string path) =>
        SKBitmap.Decode(path) ?? throw new InvalidDataException("The file is not a supported image.");

    private static SKBitmap DecodeStream(Stream stream)
    {
        using var source = new SKManagedStream(stream, disposeManagedStream: false);
        return SKBitmap.Decode(source) ?? throw new InvalidDataException("The stream is not a supported image.");
    }
}
