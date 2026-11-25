using System;

namespace OoBDev.System;

/// <summary>
/// Extension methods to support BinaryCode Decimal (BCD)
/// </summary>
public static class BcdEx
{
    /// <summary>
    /// convert Binary Code Decimal (BCD) byte to integer
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static int AsBCD(this byte input)
    {
        var low = input & 0x0f;
        var high = (input & 0xf0) >> 4;

        return low > 9
            ? throw new ArgumentOutOfRangeException(nameof(input), "low nibble is not a BCD digit")
            : high > 9 ? throw new ArgumentOutOfRangeException(nameof(input), "high nibble is not a BCD digit") : low + high * 10;
    }
    /// <summary>
    /// convert decimal value to Binary code Decimal (BCD)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static byte ToBCD(this int input)
    {
        if (input > 99 || input < 0)
            throw new ArgumentOutOfRangeException(nameof(input), "BCD input must be between 0 and 99");

        var high = input / 10;
        var low = input - high * 10;

        return low > 9
            ? throw new ArgumentOutOfRangeException(nameof(input), "low nibble is not a BCD digit")
            : high > 9 ? throw new ArgumentOutOfRangeException(nameof(input), "high nibble is not a BCD digit") : (byte)(high << 4 | low);
    }

    /// <summary>
    /// convert big-endian packed BCD bytes (two digits per byte, most significant first) to a number
    /// </summary>
    /// <param name="input">packed BCD bytes</param>
    /// <returns>the decimal value</returns>
    /// <exception cref="ArgumentOutOfRangeException">a nibble is not a BCD digit, or the value does not fit a long</exception>
    public static long AsBCD(this ReadOnlySpan<byte> input)
    {
        if (input.Length > 9)
            throw new ArgumentOutOfRangeException(nameof(input), "more than 18 BCD digits do not fit a long");

        long result = 0;
        foreach (var b in input)
            result = result * 100 + b.AsBCD();
        return result;
    }

    /// <summary>
    /// write a number as big-endian packed BCD into the destination, right aligned and zero padded on the left
    /// </summary>
    /// <param name="input">value to write, not negative</param>
    /// <param name="destination">buffer that receives the packed digits</param>
    /// <exception cref="ArgumentOutOfRangeException">the value is negative or needs more digits than the destination holds</exception>
    public static void ToBCD(this long input, Span<byte> destination)
    {
        if (input < 0)
            throw new ArgumentOutOfRangeException(nameof(input), "BCD input must not be negative");

        for (var i = destination.Length - 1; i >= 0; i--)
        {
            destination[i] = ((int)(input % 100)).ToBCD();
            input /= 100;
        }

        if (input != 0)
            throw new ArgumentOutOfRangeException(nameof(input), "value does not fit the destination");
    }
}
