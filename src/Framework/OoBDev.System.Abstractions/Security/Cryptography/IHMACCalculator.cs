using System;

namespace OoBDev.System.Security.Cryptography;

/// <summary>
/// Provides functionality for calculating HMAC (Hash-based Message Authentication Code) values.
/// </summary>
public interface IHMACCalculator
{
    /// <summary>
    /// Calculates the HMAC for the specified secret and message.
    /// </summary>
    /// <param name="secret">The secret key.</param>
    /// <param name="message">The message to authenticate.</param>
    /// <returns>The calculated HMAC as a byte span.</returns>
    ReadOnlySpan<byte> Calculate(string secret, string message);

    /// <summary>
    /// Encodes a byte span to a string representation (e.g., Base64, hexadecimal).
    /// </summary>
    /// <param name="bytes">The bytes to encode.</param>
    /// <returns>The encoded string.</returns>
    string Encode(ReadOnlySpan<byte> bytes);

    /// <summary>
    /// Calculates the HMAC and encodes it to a string in a single operation.
    /// </summary>
    /// <param name="secret">The secret key.</param>
    /// <param name="message">The message to authenticate.</param>
    /// <returns>The calculated and encoded HMAC.</returns>
    string CalculateAndEncode(string secret, string message);
}
