using System.Globalization;

namespace OoBDev.System.Numerics;

/// <summary>
/// A number shown in scientific notation with a power of ten, for example <c>1.001e3</c>.
/// </summary>
public record ScientificNumber : FormattableNumber
{
    /// <summary>Creates the number from a built-in numeric type or another <see cref="INumeric"/>.</summary>
    public ScientificNumber(object value) : base(value, new ScienceNotationFormatter())
    {
    }

    /// <summary>Converts to a <see cref="double"/>.</summary>
    public static implicit operator double(ScientificNumber number) => number.AsDouble();
    /// <summary>Converts to a <see cref="float"/>.</summary>
    public static implicit operator float(ScientificNumber number) => number.AsSingle();
    /// <summary>Converts to a <see cref="decimal"/>.</summary>
    public static implicit operator decimal(ScientificNumber number) => number.AsDecimal();
    /// <summary>Converts to a <see cref="short"/>.</summary>
    public static implicit operator short(ScientificNumber number) => number.AsInt16();
    /// <summary>Converts to an <see cref="int"/>.</summary>
    public static implicit operator int(ScientificNumber number) => number.AsInt32();
    /// <summary>Converts to a <see cref="long"/>.</summary>
    public static implicit operator long(ScientificNumber number) => number.AsInt64();

    /// <summary>Converts from a <see cref="double"/>.</summary>
    public static implicit operator ScientificNumber(double number) => new(value: number);
    /// <summary>Converts from a <see cref="float"/>.</summary>
    public static implicit operator ScientificNumber(float number) => new(value: number);
    /// <summary>Converts from a <see cref="decimal"/>.</summary>
    public static implicit operator ScientificNumber(decimal number) => new(value: number);
    /// <summary>Converts from a <see cref="short"/>.</summary>
    public static implicit operator ScientificNumber(short number) => new(value: number);
    /// <summary>Converts from an <see cref="int"/>.</summary>
    public static implicit operator ScientificNumber(int number) => new(value: number);
    /// <summary>Converts from a <see cref="long"/>.</summary>
    public static implicit operator ScientificNumber(long number) => new(value: number);

    /// <summary>Converts from a <see cref="ByteNumber"/>.</summary>
    public static implicit operator ScientificNumber(ByteNumber number) => new(value: number.AsDouble());
    /// <summary>Converts from an <see cref="EngineeringNumber"/>.</summary>
    public static implicit operator ScientificNumber(EngineeringNumber number) => new(value: number.AsDouble());

    /// <inheritdoc />
    public override string ToString() => base.ToString();

    /// <summary>Parses text such as <c>1.5e3</c> or <c>42</c>.</summary>
    /// <param name="input">The text to parse.</param>
    /// <param name="value">The parsed number, or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> when the text is a number.</returns>
    public static bool TryParse(string? input, out ScientificNumber? value)
    {
        if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
        {
            value = number;
            return true;
        }

        value = default;
        return false;
    }
}
