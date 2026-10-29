using System.Drawing;

namespace OoBDev.Extensions.Drawing;

/// <summary>Z-order (Morton) curve helpers: interleave the bits of X and Y into one index and back.</summary>
public static class ZOrderCurve
{
    /// <summary>Gathers the even-numbered bits (0, 2, 4 ...) of a value into the low 16 bits.</summary>
    public static uint Reduce(this uint value)
    {
        value &= 0x55555555;
        value = (value | (value >> 1)) & 0x33333333;
        value = (value | (value >> 2)) & 0x0F0F0F0F;
        value = (value | (value >> 4)) & 0x00FF00FF;
        return (value | (value >> 8)) & 0x0000FFFF;
    }

    /// <summary>Spreads the low 16 bits of a value onto the even-numbered bits, leaving a zero between each.</summary>
    public static uint Expand(this uint value)
    {
        value &= 0x0000FFFF;
        value = (value | (value << 8)) & 0x00FF00FF;
        value = (value | (value << 4)) & 0x0F0F0F0F;
        value = (value | (value << 2)) & 0x33333333;
        return (value | (value << 1)) & 0x55555555;
    }

    /// <summary>Gets the point (X from the even bits, Y from the odd bits) at a position on the curve.</summary>
    public static Point GetPoint(this uint value) => new((int)value.Reduce(), (int)(value >> 1).Reduce());

    /// <summary>Gets the position on the curve of a point; X and Y must be 0 to 65535.</summary>
    public static uint GetValue(this Point point) => ((uint)point.X).Expand() | (((uint)point.Y).Expand() << 1);
}
