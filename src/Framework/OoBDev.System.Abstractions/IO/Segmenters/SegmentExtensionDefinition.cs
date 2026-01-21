using System;

namespace OoBDev.System.IO.Segmenters;

/// <summary>
/// Defines how a segment's length can be determined by reading an embedded length field within the data.
/// </summary>
/// <param name="type">The data type of the length field (e.g., typeof(int), typeof(ushort)).</param>
/// <param name="length">The size of the length field in bytes.</param>
/// <param name="postion">The position within the segment where the length field is located.</param>
/// <param name="endianness">The byte order (endianness) of the length field.</param>
public class SegmentExtensionDefinition(Type type, int length, long postion, Endianness endianness)
{
    /// <summary>
    /// Gets the data type of the length field.
    /// </summary>
    public Type Type { get; } = type;

    /// <summary>
    /// Gets the size of the length field in bytes.
    /// </summary>
    public int Length { get; } = length;

    /// <summary>
    /// Gets the position within the segment where the length field is located.
    /// </summary>
    public long Postion { get; } = postion;

    /// <summary>
    /// Gets the byte order (endianness) of the length field.
    /// </summary>
    public Endianness Endianness { get; } = endianness;
}