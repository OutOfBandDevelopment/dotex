using System;
using System.Globalization;

namespace OoBDev.Media.Metadata;

/// <summary>
/// Formats numbers the way Python prints them, so dump text stays equal to rows a Pillow-based reader wrote earlier.
/// </summary>
internal static class PythonText
{
    /// <summary>Python's <c>repr(float)</c>: shortest round-trip digits, <c>.0</c> on whole numbers, exponent form outside 1e-4 and 1e16.</summary>
    public static string Float(double v)
    {
        if (double.IsNaN(v))
            return "nan";
        if (double.IsInfinity(v))
            return v > 0 ? "inf" : "-inf";
        if (v == 0)
            return double.IsNegative(v) ? "-0.0" : "0.0";

        var sign = v < 0 ? "-" : "";
        var shortest = Math.Abs(v).ToString("R", CultureInfo.InvariantCulture);
        var mantissa = shortest;
        var exponent = 0;
        var e = shortest.IndexOfAny(['E', 'e']);
        if (e >= 0)
        {
            mantissa = shortest[..e];
            exponent = int.Parse(shortest[(e + 1)..], CultureInfo.InvariantCulture);
        }

        var dot = mantissa.IndexOf('.', StringComparison.Ordinal);
        var intPart = dot < 0 ? mantissa : mantissa[..dot];
        var all = intPart + (dot < 0 ? "" : mantissa[(dot + 1)..]);
        var digits = all.TrimStart('0');
        var point = intPart.Length - (all.Length - digits.Length) + exponent;
        digits = digits.TrimEnd('0');
        if (digits.Length == 0)
            digits = "0";

        var scientific = point - 1;
        if (scientific < -4 || scientific >= 16)
        {
            var m = digits.Length == 1 ? digits : digits[0] + "." + digits[1..];
            return $"{sign}{m}e{(scientific < 0 ? "-" : "+")}{Math.Abs(scientific):00}";
        }

        if (point <= 0)
            return sign + "0." + new string('0', -point) + digits;
        if (digits.Length <= point)
            return sign + digits + new string('0', point - digits.Length) + ".0";
        return sign + digits[..point] + "." + digits[point..];
    }
}
