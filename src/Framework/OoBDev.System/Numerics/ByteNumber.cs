using System;
using System.Globalization;

namespace OoBDev.System.Numerics;

/// <summary>
/// A number shown in binary (1024) notation, for example <c>1.5kb</c>.
/// </summary>
public record ByteNumber : FormattableNumber
{
    /// <summary>Creates the number from a built-in numeric type or another <see cref="INumeric"/>.</summary>
    public ByteNumber(object value) : base(value, new ByteNotationFormatter())
    {
    }

    /// <summary>Converts to a <see cref="double"/>.</summary>
    public static implicit operator double(ByteNumber number) => number.AsDouble();
    /// <summary>Converts to a <see cref="float"/>.</summary>
    public static implicit operator float(ByteNumber number) => number.AsSingle();
    /// <summary>Converts to a <see cref="decimal"/>.</summary>
    public static implicit operator decimal(ByteNumber number) => number.AsDecimal();
    /// <summary>Converts to a <see cref="short"/>.</summary>
    public static implicit operator short(ByteNumber number) => number.AsInt16();
    /// <summary>Converts to an <see cref="int"/>.</summary>
    public static implicit operator int(ByteNumber number) => number.AsInt32();
    /// <summary>Converts to a <see cref="long"/>.</summary>
    public static implicit operator long(ByteNumber number) => number.AsInt64();

    /// <summary>Converts from a <see cref="double"/>.</summary>
    public static implicit operator ByteNumber(double number) => new(value: number);
    /// <summary>Converts from a <see cref="float"/>.</summary>
    public static implicit operator ByteNumber(float number) => new(value: number);
    /// <summary>Converts from a <see cref="decimal"/>.</summary>
    public static implicit operator ByteNumber(decimal number) => new(value: number);
    /// <summary>Converts from a <see cref="short"/>.</summary>
    public static implicit operator ByteNumber(short number) => new(value: number);
    /// <summary>Converts from an <see cref="int"/>.</summary>
    public static implicit operator ByteNumber(int number) => new(value: number);
    /// <summary>Converts from a <see cref="long"/>.</summary>
    public static implicit operator ByteNumber(long number) => new(value: number);

    /// <summary>Converts from a <see cref="ScientificNumber"/>.</summary>
    public static implicit operator ByteNumber(ScientificNumber number) => new(value: number.AsDecimal());
    /// <summary>Converts from an <see cref="EngineeringNumber"/>.</summary>
    public static implicit operator ByteNumber(EngineeringNumber number) => new(value: number.AsDecimal());

    /// <inheritdoc />
    public override string ToString() => base.ToString();

    /// <summary>Parses text such as <c>1.5kb</c> or <c>512b</c>.</summary>
    /// <param name="input">The text to parse.</param>
    /// <param name="value">The parsed number, or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> when the text is a byte-notation number.</returns>
    public static bool TryParse(string? input, out ByteNumber? value)
    {
        value = default;
        if (string.IsNullOrWhiteSpace(input) || input.Length < 2 || input[^1] != 'b')
        {
            return false;
        }

        if (Symbols.TryGetPower(input[^2], out var power))
        {
            if (!decimal.TryParse(input[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var scaled)) return false;
            try
            {
                for (var p = 0; p < power; p++) scaled *= 1024;
                for (var p = 0; p > power; p--) scaled /= 1024;
            }
            catch (OverflowException)
            {
                return false;
            }

            value = scaled;
            return true;
        }

        if (!double.TryParse(input[..^1], NumberStyles.Float, CultureInfo.InvariantCulture, out var plain)) return false;
        value = plain;
        return true;
    }
}
