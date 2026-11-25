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

    /// <summary>
    /// Calculates the exclusive-or of all bytes (the NMEA 0183 sentence checksum).
    /// </summary>
    /// <param name="buffer">The bytes to fold.</param>
    /// <returns>The 8-bit exclusive-or of <paramref name="buffer"/>; zero when it is empty.</returns>
    byte Xor8(ReadOnlySpan<byte> buffer);

    /// <summary>
    /// Calculates <c>0xFFFF - (sum of the little-endian 16-bit words % 0xFFFF)</c> over the bytes
    /// (the Radex One header and extension checksum).
    /// </summary>
    /// <param name="buffer">The bytes to sum; a trailing odd byte is ignored.</param>
    /// <returns>The 16-bit checksum.</returns>
    ushort WordSumComplement16(ReadOnlySpan<byte> buffer);
}