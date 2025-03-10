﻿using System;

namespace OoBDev.System;

/// <summary>
/// Extension methods to support BinaryCode Decimal (BCD)
/// </summary>
public static class BcdEx
{
    //TODO: extend to support ReadOnlySpan

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
            ? throw new ArgumentOutOfRangeException("low nibble")
            : high > 9 ? throw new ArgumentOutOfRangeException("high nibble") : low + high * 10;
    }
    /// <summary>
    /// convert decimal value to Binary code Decimal (BCD)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static byte ToBCD(this int input)
    {
        if (input > 99 || input < 0)
            throw new ArgumentOutOfRangeException();

        var high = input / 10;
        var low = input - high * 10;

        return low > 9
            ? throw new ArgumentOutOfRangeException("low nibble")
            : high > 9 ? throw new ArgumentOutOfRangeException("high nibble") : (byte)(high << 4 | low);
    }
}
