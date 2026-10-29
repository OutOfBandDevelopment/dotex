using System;
using System.Globalization;

namespace OoBDev.System.Numerics;

/// <summary>
/// Formats a number in engineering notation with an SI prefix, for example <c>1.001k</c> or <c>110m</c>.
/// </summary>
public sealed class EngineeringNotationFormatter : IFormatProvider, ICustomFormatter
{
    /// <inheritdoc />
    public string Format(string? format, object? arg, IFormatProvider? formatProvider)
    {
        var value = Convert.ToDouble(arg, formatProvider);
        if (value == 0 || !double.IsFinite(value))
        {
            return Symbols.Scaled(value, format);
        }

        var power = (int)global::System.Math.Floor(global::System.Math.Log10(global::System.Math.Abs(value)) / 3);
        var symbol = Symbols.ForPower(power) ?? $"e{(power * 3).ToString(CultureInfo.InvariantCulture)}";

        return Symbols.Scaled(value * global::System.Math.Pow(1000, -power), format) + symbol;
    }

    /// <inheritdoc />
    public object? GetFormat(Type? formatType) =>
        formatType == typeof(ICustomFormatter) ? this : null;
}
