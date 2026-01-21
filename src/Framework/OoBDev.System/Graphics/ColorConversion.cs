using OoBDev.System.Linq;
using System.Linq;

namespace OoBDev.System.Graphics;

/// <summary>
/// Provides color space conversion utilities for converting between RGB, HSL, and HSV color representations.
/// </summary>
public static class ColorConversion
{
    /// <summary>
    /// Converts RGB color values (bytes) to HSL color space.
    /// </summary>
    /// <param name="color">The RGB color as a tuple of byte values (0-255).</param>
    /// <returns>A tuple containing hue (0-360), saturation (0-1), and lightness (0-1).</returns>
    public static (double hue, double saturation, double lightness) Rgb2Hsl((byte red, byte green, byte blue) color) => Rgb2Hsl(color.red, color.green, color.blue);

    /// <summary>
    /// Converts RGB color values (bytes) to HSL color space.
    /// </summary>
    /// <param name="red">The red component (0-255).</param>
    /// <param name="green">The green component (0-255).</param>
    /// <param name="blue">The blue component (0-255).</param>
    /// <returns>A tuple containing hue (0-360), saturation (0-1), and lightness (0-1).</returns>
    public static (double hue, double saturation, double lightness) Rgb2Hsl(byte red, byte green, byte blue) => Rgb2Hsl((red, green, blue), 255.0);

    /// <summary>
    /// Converts RGB color values (doubles) to HSL color space with a scaling factor.
    /// </summary>
    /// <param name="color">The RGB color as a tuple of double values.</param>
    /// <param name="factor">The scaling factor for the input values (default is 1.0). Use 255.0 for byte-range values.</param>
    /// <returns>A tuple containing hue (0-360), saturation (0-1), and lightness (0-1).</returns>
    public static (double hue, double saturation, double lightness) Rgb2Hsl((double red, double green, double blue) color, double factor = 1.0)
    {
        var primes = (red: color.red / factor, green: color.green / factor, blue: color.blue / factor);
        var c = (max: primes.ToArray<double>().Max(), min: primes.ToArray<double>().Min());
        var diff = c.max - c.min;
        var lightness = (c.max + c.min) / 2.0;

        return (
            hue: diff switch
            {
                0.0 => 0.0,
                _ when c.max == primes.red => (primes.green - primes.blue) / diff % 6,
                _ when c.max == primes.green => (primes.blue - primes.red) / diff + 2,
                _ when c.max == primes.blue => (primes.red - primes.green) / diff + 4,
                _ => 0.0
            } * 60 % 360,
            saturation: diff switch
            {
                0.0 => 0.0,
                _ => diff / (1.0 - global::System.Math.Abs(2 * lightness - 1))
            }, lightness
            );
    }

    /// <summary>
    /// Converts RGB color values (bytes) to HSV color space.
    /// </summary>
    /// <param name="color">The RGB color as a tuple of byte values (0-255).</param>
    /// <returns>A tuple containing hue (0-360), saturation (0-1), and value (0-1).</returns>
    public static (double hue, double saturation, double value) Rgb2Hsv((byte red, byte green, byte blue) color) => Rgb2Hsv(color.red, color.green, color.blue);

    /// <summary>
    /// Converts RGB color values (bytes) to HSV color space.
    /// </summary>
    /// <param name="red">The red component (0-255).</param>
    /// <param name="green">The green component (0-255).</param>
    /// <param name="blue">The blue component (0-255).</param>
    /// <returns>A tuple containing hue (0-360), saturation (0-1), and value (0-1).</returns>
    public static (double hue, double saturation, double value) Rgb2Hsv(byte red, byte green, byte blue) => Rgb2Hsv((red, green, blue), 255.0);

    /// <summary>
    /// Converts RGB color values (doubles) to HSV color space with a scaling factor.
    /// </summary>
    /// <param name="color">The RGB color as a tuple of double values.</param>
    /// <param name="factor">The scaling factor for the input values (default is 1.0). Use 255.0 for byte-range values.</param>
    /// <returns>A tuple containing hue (0-360), saturation (0-1), and value (0-1).</returns>
    public static (double hue, double saturation, double value) Rgb2Hsv((double red, double green, double blue) color, double factor = 1.0)
    {
        var primes = (red: color.red / factor, green: color.green / factor, blue: color.blue / factor);
        var c = (max: primes.ToArray<double>().Max(), min: primes.ToArray<double>().Min());
        var diff = c.max - c.min;

        return (
            hue: diff switch
            {
                0.0 => 0.0,
                _ when c.max == primes.red => (primes.green - primes.blue) / diff % 6,
                _ when c.max == primes.green => (primes.blue - primes.red) / diff + 2,
                _ when c.max == primes.blue => (primes.red - primes.green) / diff + 4,
                _ => 0.0
            } * 60 % 360,
            saturation: c.max switch
            {
                0.0 => 0.0,
                _ => diff / c.max
            },
            value: c.max
            );
    }

    /// <summary>
    /// Converts HSV color values to RGB color space with a scaling factor.
    /// </summary>
    /// <param name="color">The HSV color as a tuple (hue in 0-360, saturation in 0-1, value in 0-1).</param>
    /// <param name="factor">The scaling factor for the output values (default is 1.0). Use 255.0 for byte-range values.</param>
    /// <returns>A tuple containing red, green, and blue values scaled by the factor.</returns>
    public static (double red, double green, double blue) Hsv2Rgb((double hue, double saturation, double value) color, double factor = 1.0)
    {
        var adjusted = (hue: color.hue % 360.0, color.saturation, color.value);

        var c = adjusted.value * adjusted.saturation;
        var x = c * (1 - global::System.Math.Abs(adjusted.hue / 60 % 2 - 1));
        var m = adjusted.value - c;

        (double red, double green, double blue) rgb = ((int)(adjusted.hue / 60) % 6) switch
        {
            0 => (c, x, 0),
            1 => (x, c, 0),
            2 => (0, c, x),
            3 => (0, x, c),
            4 => (x, 0, c),
            _ => (c, 0, x)
        };

        return ((rgb.red + m) * factor, (rgb.green + m) * factor, (rgb.blue + m) * factor);
    }

    /// <summary>
    /// Converts HSL color values to RGB color space with a scaling factor.
    /// </summary>
    /// <param name="color">The HSL color as a tuple (hue in 0-360, saturation in 0-1, lightness in 0-1).</param>
    /// <param name="factor">The scaling factor for the output values (default is 1.0). Use 255.0 for byte-range values.</param>
    /// <returns>A tuple containing red, green, and blue values scaled by the factor.</returns>
    public static (double red, double green, double blue) Hsl2Rgb((double hue, double saturation, double lightness) color, double factor = 1.0)
    {
        var adjusted = (hue: color.hue % 360.0, color.saturation, color.lightness);

        var c = (1 - global::System.Math.Abs(2.0 * adjusted.lightness - 1.0)) * adjusted.saturation;
        ;
        var x = c * (1 - global::System.Math.Abs(adjusted.hue / 60 % 2 - 1));
        var m = adjusted.lightness - c / 2.0;

        (double red, double green, double blue) rgb = ((int)(adjusted.hue / 60) % 6) switch
        {
            0 => (c, x, 0),
            1 => (x, c, 0),
            2 => (0, c, x),
            3 => (0, x, c),
            4 => (x, 0, c),
            _ => (c, 0, x)
        };

        return ((rgb.red + m) * factor, (rgb.green + m) * factor, (rgb.blue + m) * factor);
    }

    /// <summary>
    /// Converts HSL color values to HSV color space.
    /// </summary>
    /// <param name="color">The HSL color as a tuple (hue in 0-360, saturation in 0-1, lightness in 0-1).</param>
    /// <returns>A tuple containing hue (0-360), saturation (0-1), and value (0-1) in HSV color space.</returns>
    public static (double hue, double saturation, double value) Hsl2Hsv((double hue, double saturation, double lightness) color)
    {
        var adjusted = (hue: color.hue % 360.0, color.saturation, color.lightness);

        var c = (1 - global::System.Math.Abs(2.0 * adjusted.lightness - 1.0)) * adjusted.saturation;
        var x = c * (1 - global::System.Math.Abs(adjusted.hue / 60 % 2 - 1));
        var m = adjusted.lightness - c / 2.0;

        var terms = new[] { c + m, x + m, m };

        var c2 = (max: terms.Max(), min: terms.Min());
        var diff = c2.max - c2.min;

        return (adjusted.hue,
            saturation: c2.max == 0.0 ? 0.0 : diff / c2.max,
            value: c2.max
            );
    }

    /// <summary>
    /// Converts HSV color values to HSL color space.
    /// </summary>
    /// <param name="color">The HSV color as a tuple (hue in 0-360, saturation in 0-1, value in 0-1).</param>
    /// <returns>A tuple containing hue (0-360), saturation (0-1), and lightness (0-1) in HSL color space.</returns>
    public static (double hue, double saturation, double lightness) Hsv2Hsl((double hue, double saturation, double value) color)
    {
        var adjusted = (hue: color.hue % 360.0, color.saturation, color.value);

        var c = adjusted.value * adjusted.saturation;
        var x = c * (1 - global::System.Math.Abs(adjusted.hue / 60 % 2 - 1));
        var m = adjusted.value - c;

        var rgbP = new[] { c + m, x + m, m };
        var c2 = (max: rgbP.Max(), min: rgbP.Min());
        var diff = c2.max - c2.min;
        var lightness = (c2.max + c2.min) / 2.0;

        return (adjusted.hue,
            saturation: diff == 0.0 ? 0.0 : diff / (1.0 - global::System.Math.Abs(2 * lightness - 1)), lightness
            );
    }
}
