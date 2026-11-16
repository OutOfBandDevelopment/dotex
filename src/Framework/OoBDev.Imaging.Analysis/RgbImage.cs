using System;

namespace OoBDev.Imaging.Analysis;

/// <summary>An immutable view of an 8-bit RGB image (3 bytes a pixel, row-major). Decoding stays with the caller.</summary>
public sealed class RgbImage
{
    /// <summary>Creates the view; the buffer is not copied.</summary>
    public RgbImage(int width, int height, ReadOnlyMemory<byte> rgb)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        if (rgb.Length != checked(width * height * 3))
            throw new ArgumentException("The buffer length does not match the size.", nameof(rgb));
        Width = width;
        Height = height;
        Rgb = rgb;
    }

    /// <summary>Width in pixels.</summary>
    public int Width { get; }

    /// <summary>Height in pixels.</summary>
    public int Height { get; }

    /// <summary>The pixel bytes: red, green, blue for each pixel.</summary>
    public ReadOnlyMemory<byte> Rgb { get; }

    /// <summary>Number of pixels.</summary>
    public int PixelCount => Width * Height;

    /// <summary>Rec. 709 luminance of a pixel, 0 to 255, rounded.</summary>
    public static byte Luminance(byte r, byte g, byte b) => (byte)Math.Round((0.2126 * r) + (0.7152 * g) + (0.0722 * b), MidpointRounding.AwayFromZero);

    /// <summary>A new gray image (all three channels the luminance).</summary>
    public RgbImage ToGrayscale()
    {
        var src = Rgb.Span;
        var dst = new byte[src.Length];
        for (var i = 0; i < src.Length; i += 3)
            dst[i] = dst[i + 1] = dst[i + 2] = Luminance(src[i], src[i + 1], src[i + 2]);
        return new RgbImage(Width, Height, dst);
    }

    /// <summary>A new image with <c>value * contrast + brightness</c> on each channel, clamped to 0..255.</summary>
    public RgbImage AdjustBrightnessContrast(double brightness, double contrast)
    {
        var src = Rgb.Span;
        var dst = new byte[src.Length];
        for (var i = 0; i < src.Length; i++)
            dst[i] = (byte)Math.Clamp(Math.Round((src[i] * contrast) + brightness, MidpointRounding.AwayFromZero), 0, 255);
        return new RgbImage(Width, Height, dst);
    }
}
