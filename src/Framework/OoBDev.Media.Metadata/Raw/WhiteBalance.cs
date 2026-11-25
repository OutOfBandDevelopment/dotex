using System;

namespace OoBDev.Media.Metadata.Raw;

/// <summary>White-balance scaling of an interleaved 8-bit RGB buffer, the step a native developer applies before encoding.</summary>
public static class WhiteBalance
{
    /// <summary>Multiplies each channel by its scale and clamps to 0..255; returns a new buffer.</summary>
    /// <param name="rgb">Interleaved R, G, B bytes; the length must be a multiple of three.</param>
    /// <param name="red">The red multiplier.</param>
    /// <param name="green">The green multiplier.</param>
    /// <param name="blue">The blue multiplier.</param>
    public static byte[] Scale(ReadOnlySpan<byte> rgb, double red, double green, double blue)
    {
        if (rgb.Length % 3 != 0)
        {
            throw new ArgumentException("The buffer length must be a multiple of three.", nameof(rgb));
        }

        var scales = new[] { red, green, blue };
        var result = new byte[rgb.Length];
        for (var i = 0; i < rgb.Length; i++)
        {
            var value = Math.Round(rgb[i] * scales[i % 3]);
            result[i] = (byte)Math.Clamp(value, 0, 255);
        }

        return result;
    }
}
