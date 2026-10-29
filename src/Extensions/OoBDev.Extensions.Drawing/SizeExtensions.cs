using System;
using System.Drawing;

namespace OoBDev.Extensions.Drawing;

/// <summary>Size and point arithmetic used by the multi-scale tile pyramid.</summary>
public static class SizeExtensions
{
    /// <summary>Gets the highest pyramid level: the smallest power of two that covers the longer side, as an exponent.</summary>
    public static int MaxLevel(this Size size) => (int)Math.Ceiling(Math.Log2(Math.Max(1, Math.Max(size.Width, size.Height))));

    /// <summary>Scales both sides by a factor, rounding up and never going below one pixel.</summary>
    public static Size Scale(this Size size, double factor) => new(
        (int)Math.Max(1, Math.Ceiling(size.Width * factor)),
        (int)Math.Max(1, Math.Ceiling(size.Height * factor)));

    /// <summary>Multiplies a grid position by a cell size, giving the pixel position of the cell.</summary>
    public static Point OffsetBy(this Point point, Size size) => new(point.X * size.Width, point.Y * size.Height);
}
