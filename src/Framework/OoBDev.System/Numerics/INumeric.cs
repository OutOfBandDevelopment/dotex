using System;

namespace OoBDev.System.Numerics;

/// <summary>
/// A number that can be read as any of the common numeric types and formatted with its own notation.
/// </summary>
public interface INumeric : IFormattable
{
    /// <summary>Gets the value as a <see cref="double"/>.</summary>
    double AsDouble();

    /// <summary>Gets the value as a <see cref="float"/>.</summary>
    float AsSingle();

    /// <summary>Gets the value as a <see cref="decimal"/>.</summary>
    decimal AsDecimal();

    /// <summary>Gets the value as a <see cref="short"/>, truncating any fraction.</summary>
    short AsInt16();

    /// <summary>Gets the value as an <see cref="int"/>, truncating any fraction.</summary>
    int AsInt32();

    /// <summary>Gets the value as a <see cref="long"/>, truncating any fraction.</summary>
    long AsInt64();
}
