using System;
using System.Globalization;

namespace OoBDev.System.Numerics;

/// <summary>
/// A number shown in engineering notation with an SI prefix, for example <c>110m</c>.
/// </summary>
public record EngineeringNumber : FormattableNumber
{
    /// <summary>Creates the number from a built-in numeric type or another <see cref="INumeric"/>.</summary>
    public EngineeringNumber(object value) : base(value, new EngineeringNotationFormatter())
    {
    }

    /// <summary>Converts to a <see cref="double"/>.</summary>
    public static implicit operator double(EngineeringNumber number) => number.AsDouble();
    /// <summary>Converts to a <see cref="float"/>.</summary>
    public static implicit operator float(EngineeringNumber number) => number.AsSingle();
    /// <summary>Converts to a <see cref="decimal"/>.</summary>
    public static implicit operator decimal(EngineeringNumber number) => number.AsDecimal();
    /// <summary>Converts to a <see cref="short"/>.</summary>
    public static implicit operator short(EngineeringNumber number) => number.AsInt16();
    /// <summary>Converts to an <see cref="int"/>.</summary>
    public static implicit operator int(EngineeringNumber number) => number.AsInt32();
    /// <summary>Converts to a <see cref="long"/>.</summary>
    public static implicit operator long(EngineeringNumber number) => number.AsInt64();

    /// <summary>Converts from a <see cref="double"/>.</summary>
    public static implicit operator EngineeringNumber(double number) => new(value: number);
    /// <summary>Converts from a <see cref="float"/>.</summary>
    public static implicit operator EngineeringNumber(float number) => new(value: number);
    /// <summary>Converts from a <see cref="decimal"/>.</summary>
    public static implicit operator EngineeringNumber(decimal number) => new(value: number);
    /// <summary>Converts from a <see cref="short"/>.</summary>
    public static implicit operator EngineeringNumber(short number) => new(value: number);
    /// <summary>Converts from an <see cref="int"/>.</summary>
    public static implicit operator EngineeringNumber(int number) => new(value: number);
    /// <summary>Converts from a <see cref="long"/>.</summary>
    public static implicit operator EngineeringNumber(long number) => new(value: number);

    /// <summary>Converts from a <see cref="ScientificNumber"/>.</summary>
    public static implicit operator EngineeringNumber(ScientificNumber number) => new(value: number.AsDecimal());
    /// <summary>Converts from a <see cref="ByteNumber"/>.</summary>
    public static implicit operator EngineeringNumber(ByteNumber number) => new(value: number.AsDecimal());

    /// <inheritdoc />
    public override string ToString() => base.ToString();

    /// <summary>Parses text such as <c>1.5k</c>, <c>110m</c> or <c>42</c>.</summary>
    /// <param name="input">The text to parse.</param>
    /// <param name="value">The parsed number, or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> when the text is an engineering-notation number.</returns>
    public static bool TryParse(string? input, out EngineeringNumber? value)
    {
        value = default;
        if (string.IsNullOrWhiteSpace(input)) return false;

        if (Symbols.TryGetPower(input[^1], out var power))
        {
            if (!decimal.TryParse(input[..^1], NumberStyles.Float, CultureInfo.InvariantCulture, out var scaled)) return false;
            try
            {
                for (var p = 0; p < power; p++) scaled *= 1000;
                for (var p = 0; p > power; p--) scaled /= 1000;
            }
            catch (OverflowException)
            {
                return false;
            }

            value = scaled;
            return true;
        }

        if (!double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out var plain)) return false;
        value = plain;
        return true;
    }
}
