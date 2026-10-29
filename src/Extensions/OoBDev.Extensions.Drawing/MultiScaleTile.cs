using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Versioning;

namespace OoBDev.Extensions.Drawing;

/// <summary>
/// Cuts an image into a pyramid of square tiles, as used by deep-zoom viewers. Level <c>n</c> shows the whole image
/// at scale <c>2^n / 2^max</c>; the top level (<see cref="GetMaxLevel(Image)"/>) is full size.
/// </summary>
[SupportedOSPlatform("windows")]
public static class MultiScaleTile
{
    /// <summary>The usual tile edge length in pixels.</summary>
    public const int DefaultTileSize = 256;

    /// <summary>Gets the top level of the pyramid for the image.</summary>
    public static int GetMaxLevel(this Image image)
    {
        ArgumentNullException.ThrowIfNull(image);
        return image.Size.MaxLevel();
    }

    /// <summary>Gets how many tiles wide and high a level is.</summary>
    public static Size GetTileCount(this Image image, int level, int tileSize = DefaultTileSize)
    {
        ArgumentNullException.ThrowIfNull(image);
        var factor = Math.Pow(2, level - image.Size.MaxLevel());
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
    public static Image GetTile(this Image image, int level, int x, int y, int tileSize = DefaultTileSize)
    {
        ArgumentNullException.ThrowIfNull(image);

        var point = new Point(x, y);
        var maxLevelLength = Math.Pow(2, image.Size.MaxLevel());
        var levelLength = Math.Pow(2, level);
        var levelScale = tileSize / levelLength;

        if (levelScale >= 1)
            return new Bitmap(image, image.Size.Scale(levelLength / maxLevelLength));

        var maxLevelSize = new Size((int)maxLevelLength, (int)maxLevelLength);
        var extractSize = maxLevelSize.Scale(levelScale);
        var extract = new Rectangle(point.OffsetBy(extractSize), extractSize);

        var tile = new Bitmap(tileSize, tileSize);
        try
        {
            using var graphics = Graphics.FromImage(tile);
            graphics.DrawImage(image, new Rectangle(0, 0, tileSize, tileSize), extract, GraphicsUnit.Pixel);
        }
        catch
        {
            tile.Dispose();
            throw;
        }

        return tile;
    }

    /// <summary>Renders one tile and encodes it as JPEG.</summary>
    public static byte[] GetTileAsBytes(this Image image, int level, int x, int y, int tileSize = DefaultTileSize)
    {
        using var tile = image.GetTile(level, x, y, tileSize);
        using var output = new MemoryStream();
        tile.Save(output, ImageFormat.Jpeg);
        return output.ToArray();
    }

    /// <summary>Renders one tile as a JPEG stream positioned at the start; the caller disposes it.</summary>
    public static Stream GetTileAsStream(this Image image, int level, int x, int y, int tileSize = DefaultTileSize) =>
        new MemoryStream(image.GetTileAsBytes(level, x, y, tileSize), writable: false);

    /// <summary>Gets the top level of the pyramid for the image file.</summary>
    public static int GetMaxLevel(this string imagePath)
    {
        using var image = Image.FromFile(imagePath);
        return image.GetMaxLevel();
    }

    /// <summary>Gets how many tiles wide and high a level is for the image file.</summary>
    public static Size GetTileCount(this string imagePath, int level, int tileSize = DefaultTileSize)
    {
        using var image = Image.FromFile(imagePath);
        return image.GetTileCount(level, tileSize);
    }

    /// <summary>Renders one tile of the image file and encodes it as JPEG.</summary>
    public static byte[] GetTileAsBytes(this string imagePath, int level, int x, int y, int tileSize = DefaultTileSize)
    {
        using var image = Image.FromFile(imagePath);
        return image.GetTileAsBytes(level, x, y, tileSize);
    }

    /// <summary>Gets the top level of the pyramid for the image in a stream.</summary>
    public static int GetMaxLevel(this Stream imageStream)
    {
        using var image = Image.FromStream(imageStream);
        return image.GetMaxLevel();
    }

    /// <summary>Gets how many tiles wide and high a level is for the image in a stream.</summary>
    public static Size GetTileCount(this Stream imageStream, int level, int tileSize = DefaultTileSize)
    {
        using var image = Image.FromStream(imageStream);
        return image.GetTileCount(level, tileSize);
    }

    /// <summary>Renders one tile of the image in a stream and encodes it as JPEG.</summary>
    public static byte[] GetTileAsBytes(this Stream imageStream, int level, int x, int y, int tileSize = DefaultTileSize)
    {
        using var image = Image.FromStream(imageStream);
        return image.GetTileAsBytes(level, x, y, tileSize);
    }
}
