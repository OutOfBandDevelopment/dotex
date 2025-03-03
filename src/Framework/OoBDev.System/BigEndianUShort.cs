using System;
using System.Runtime.InteropServices;

namespace OoBDev.System;

/// <summary>
/// Structure type to support conversion from unsigned Big Endian 16bit values
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 0x2)]
public readonly struct BigEndianUShort
{
    /// <summary>
    /// convert little endian 16bit value to big endian
    /// </summary>
    /// <param name="input"></param>
    public BigEndianUShort(ushort input)
    {
        HH = (byte)(input >> 8);
        LL = (byte)input;
    }
    /// <summary>
    /// create unsigned big endian 16bit from ReadOnlySpan&lt;byte&gt;
    /// </summary>
    /// <param name="input"></param>
    public BigEndianUShort(ReadOnlySpan<byte> input)
    {
        HH = input[0];
        LL = input[1];
    }

    /// <summary>
    /// High byte for big Endian 16bit value
    /// </summary>
    [FieldOffset(0)]
    public readonly byte HH;
    /// <summary>
    /// Low byte for big Endian 16bit value
    /// </summary>
    [FieldOffset(1)]
    public readonly byte LL;

    /// <summary>
    /// Converted big Endian to little Endian
    /// </summary>
    public ushort Value => (ushort)(HH << 8 | LL);

    /// <summary>
    /// Returns a string representation of the value.
    /// </summary>
    /// <returns>A string that represents the value.</returns>
    public override string ToString() => Value.ToString();
    
    /// <summary>
    /// Checks if the given object is equal to the current <see cref="BigEndianUShort"/>.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns><c>true</c> if the object is equal to the current <see cref="BigEndianUShort"/>; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj) => Value.Equals(obj);
   
    /// <summary>
    /// Gets the hash code for the current <see cref="BigEndianUShort"/>.
    /// </summary>
    /// <returns>The hash code of the value.</returns>
   public override int GetHashCode() => Value.GetHashCode();

    /// <summary>
    /// Implicitly converts a <see cref="BigEndianUShort"/> to a <see cref="ushort"/>.
    /// </summary>
    /// <param name="input">The <see cref="BigEndianUShort"/> to convert.</param>
    public static implicit operator ushort(BigEndianUShort input) => input.Value;

    /// <summary>
    /// Implicitly converts a <see cref="ushort"/> to a <see cref="BigEndianUShort"/>.
    /// </summary>
    /// <param name="input">The <see cref="ushort"/> to convert.</param>
    public static implicit operator BigEndianUShort(ushort input) => new(input);

    /// <summary>
    /// Implicitly converts a <see cref="BigEndianUShort"/> to an <see cref="int"/>.
    /// </summary>
    /// <param name="input">The <see cref="BigEndianUShort"/> to convert.</param>
    public static implicit operator int(BigEndianUShort input) => input.Value;
   
    /// <summary>
    /// Explicitly converts an <see cref="int"/> to a <see cref="BigEndianUShort"/>.
    /// </summary>
    /// <param name="input">The <see cref="int"/> to convert.</param>
    public static explicit operator BigEndianUShort(int input) => new((ushort)input);

    /// <summary>
    /// Compares two <see cref="BigEndianUShort"/> instances for equality.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns><c>true</c> if the two instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(BigEndianUShort left, BigEndianUShort right) => left.Equals(right);

    /// <summary>
    /// Compares two <see cref="BigEndianUShort"/> instances for inequality.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns><c>true</c> if the two instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(BigEndianUShort left, BigEndianUShort right) => !(left == right);
}
