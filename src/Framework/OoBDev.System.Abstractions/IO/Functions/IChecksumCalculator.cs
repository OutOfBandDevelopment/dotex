using System;

namespace OoBDev.System.IO.Functions;

/// <summary>
/// Provides methods for calculating checksums.
/// </summary>
public interface IChecksumCalculator
{
    /// <summary>
    /// Calculates a simple 16-bit checksum for the specified buffer.
    /// </summary>
    /// <param name="buffer">The buffer containing the data to calculate the checksum for.</param>
    /// <returns>The calculated 16-bit checksum.</returns>
    ushort Simple16(ReadOnlySpan<ushort> buffer);
}