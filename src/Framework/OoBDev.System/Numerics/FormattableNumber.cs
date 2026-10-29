using System;
using System.Globalization;

namespace OoBDev.System.Numerics;

/// <summary>
/// A number that keeps its original precision and formats itself through a notation formatter.
/// </summary>
public record FormattableNumber : INumeric
{
    private readonly double? _double;
    private readonly decimal? _decimal;

    /// <summary>The format provider used when none is passed to <see cref="ToString(string?, IFormatProvider?)"/>.</summary>
    protected IFormatProvider Provider { get; }

    /// <summary>Creates a number from a built-in numeric type or another <see cref="INumeric"/>.</summary>
    /// <param name="value">A <see cref="double"/>, <see cref="float"/>, <see cref="decimal"/>, <see cref="short"/>, <see cref="int"/>, <see cref="long"/> or <see cref="INumeric"/>.</param>
    /// <param name="provider">The notation formatter; scientific notation when omitted.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException">The type of <paramref name="value"/> is not supported.</exception>
    public FormattableNumber(object value, IFormatProvider? provider = default)
    {
        ArgumentNullException.ThrowIfNull(value);

        Provider = provider ?? new ScienceNotationFormatter();

        switch (value)
        {
            case double or float or short or int or long:
                _double = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                break;
            case decimal number:
                _decimal = number;
                break;
            case FormattableNumber formatted:
                Provider = provider ?? formatted.Provider;
                _double = formatted._double;
                _decimal = formatted._decimal;
                break;
            case INumeric numeric:
                _decimal = numeric.AsDecimal();
                break;
            default:
                throw new NotSupportedException($"type {value.GetType()} is not supported");
        }
    }

    /// <inheritdoc />
    public double AsDouble() => _double ?? (double)(_decimal ?? default);

    /// <inheritdoc />
    public float AsSingle() => (float)AsDouble();

    /// <inheritdoc />
    public decimal AsDecimal() => _decimal ?? (decimal)(_double ?? default);

    /// <inheritdoc />
    public short AsInt16() => (short)AsInt64();

    /// <inheritdoc />
    public int AsInt32() => (int)AsInt64();

    /// <inheritdoc />
    public long AsInt64() => _decimal.HasValue ? (long)_decimal.Value : (long)(_double ?? default);

    /// <summary>
    /// Parses text in byte (<c>1.5kb</c>), engineering (<c>110m</c>) or scientific (<c>1e3</c>) notation.
    /// </summary>
    /// <param name="input">The text to parse.</param>
    /// <returns>The number, or <see langword="null"/> when <paramref name="input"/> is empty.</returns>
    /// <exception cref="NotSupportedException">The text matches none of the notations.</exception>
    public static INumeric? Parse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return default;
        if (ByteNumber.TryParse(input, out var byteNumber)) return byteNumber;
        if (EngineeringNumber.TryParse(input, out var engineeringNumber)) return engineeringNumber;
        if (ScientificNumber.TryParse(input, out var scientificNumber)) return scientificNumber;
        throw new NotSupportedException("Invalid input format");
    }

    /// <inheritdoc />
    public override string ToString() => ToString(null, Provider);

    /// <summary>Formats the number with the notation formatter and the given numeric format.</summary>
    public virtual string ToString(string? format) => ToString(format, Provider);

    /// <inheritdoc />
    public virtual string ToString(string? format, IFormatProvider? formatProvider)
    {
        var provider = formatProvider ?? Provider;
        return provider.GetFormat(typeof(ICustomFormatter)) is ICustomFormatter formatter
            ? formatter.Format(format, AsDouble(), provider)
            : AsDouble().ToString(format, provider);
    }
}
