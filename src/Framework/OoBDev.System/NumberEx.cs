namespace OoBDev.System;

/// <summary>
/// Provides extension methods for parsing strings into numeric types with null-safe conversions.
/// </summary>
public static class NumberEx
{
    /// <summary>
    /// Attempts to parse a string into a single-precision floating-point number.
    /// </summary>
    /// <param name="input">The string to parse.</param>
    /// <returns>The parsed float value if successful; otherwise, null.</returns>
    public static float? ToFloat(this string input) =>
        float.TryParse(input, out var ret) ? ret : null;

    /// <summary>
    /// Attempts to parse a string into a 32-bit signed integer.
    /// </summary>
    /// <param name="input">The string to parse.</param>
    /// <returns>The parsed integer value if successful; otherwise, null.</returns>
    public static int? ToInteger(this string input) =>
        int.TryParse(input, out var ret) ? ret : null;

    /// <summary>
    /// Attempts to parse a string into a decimal number.
    /// </summary>
    /// <param name="input">The string to parse.</param>
    /// <returns>The parsed decimal value if successful; otherwise, null.</returns>
    public static decimal? ToDecimal(this string input) =>
        decimal.TryParse(input, out var ret) ? ret : null;

    /// <summary>
    /// Attempts to parse a string into a double-precision floating-point number.
    /// Supports standard numeric formats and fractional notation (e.g., "1/125" is parsed as 0.008).
    /// </summary>
    /// <param name="input">The string to parse.</param>
    /// <returns>The parsed double value if successful; otherwise, null.</returns>
    public static double? ToDouble(this string input)
    {
        if (double.TryParse(input, out var ret))
            return ret;
        else if (input?.Trim().StartsWith("1/") ?? false)
            if (double.TryParse(input.Trim()[2..], out ret))
                return 1d / ret;

        return null;
    }
}
