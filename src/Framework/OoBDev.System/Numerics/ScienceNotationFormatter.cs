using System;

namespace OoBDev.System.Numerics;

/// <summary>
/// Formats a number in scientific notation with a power of ten, for example <c>1.001e3</c>.
/// </summary>
public sealed class ScienceNotationFormatter : IFormatProvider, ICustomFormatter
{
    /// <inheritdoc />
    public string Format(string? format, object? arg, IFormatProvider? formatProvider)
    {
        var value = Convert.ToDouble(arg, formatProvider);
        if (value == 0 || !double.IsFinite(value))
        {
            return Symbols.Scaled(value, format);
        }

        var exponent = (int)global::System.Math.Floor(global::System.Math.Log10(global::System.Math.Abs(value)));
        var suffix = exponent == 0 ? "" : $"e{exponent}";

        return Symbols.Scaled(value * global::System.Math.Pow(10, -exponent), format) + suffix;
    }

    /// <inheritdoc />
    public object? GetFormat(Type? formatType) =>
        formatType == typeof(ICustomFormatter) ? this : null;
}
