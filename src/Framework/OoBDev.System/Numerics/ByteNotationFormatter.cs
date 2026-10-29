using System;
using System.Globalization;

namespace OoBDev.System.Numerics;

/// <summary>
/// Formats a number in binary (1024) notation with an SI prefix and a trailing <c>b</c>, for example <c>1kb</c>.
/// </summary>
public sealed class ByteNotationFormatter : IFormatProvider, ICustomFormatter
{
    /// <inheritdoc />
    public string Format(string? format, object? arg, IFormatProvider? formatProvider)
    {
        var value = Convert.ToDouble(arg, formatProvider);
        if (value == 0 || !double.IsFinite(value))
        {
            return Symbols.Scaled(value, format) + 'b';
        }

        var power = (int)global::System.Math.Floor(global::System.Math.Log(global::System.Math.Abs(value), 1024));
        var symbol = Symbols.ForPower(power) ?? $"e{(power * 3).ToString(CultureInfo.InvariantCulture)}";

        return Symbols.Scaled(value * global::System.Math.Pow(1024, -power), format) + symbol + 'b';
    }

    /// <inheritdoc />
    public object? GetFormat(Type? formatType) =>
        formatType == typeof(ICustomFormatter) ? this : null;
}
