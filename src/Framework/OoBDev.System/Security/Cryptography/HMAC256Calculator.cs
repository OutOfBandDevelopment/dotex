using System;
using System.Security.Cryptography;
using System.Text;

namespace OoBDev.System.Security.Cryptography;

/// <summary>
/// Calculates HMAC-SHA256 hashes for message authentication.
/// </summary>
public class HMAC256Calculator : IHMACCalculator
{
    /// <summary>
    /// Calculates the HMAC-SHA256 hash of a message using the provided secret key.
    /// </summary>
    /// <param name="secret">The secret key for HMAC calculation.</param>
    /// <param name="message">The message to hash.</param>
    /// <returns>The HMAC-SHA256 hash as a byte span.</returns>
    public ReadOnlySpan<byte> Calculate(string secret, string message)
    {
        var secretBytes = Encoding.UTF8.GetBytes(secret);
        var uriBytes = Encoding.UTF8.GetBytes(message);
        using var hmac = new HMACSHA256(secretBytes);
        var hashBytes = hmac.ComputeHash(uriBytes);
        return hashBytes;
    }

    /// <summary>
    /// Encodes a byte array as a Base64 string.
    /// </summary>
    /// <param name="bytes">The bytes to encode.</param>
    /// <returns>The Base64-encoded string.</returns>
    public string Encode(ReadOnlySpan<byte> bytes)
    {
        var hashInBase64 = Convert.ToBase64String(bytes);
        var hashInBase64URIencoded = hashInBase64; ;
        return hashInBase64URIencoded;
    }

    /// <summary>
    /// Calculates the HMAC-SHA256 hash and encodes it as a Base64 string in one operation.
    /// </summary>
    /// <param name="secret">The secret key for HMAC calculation.</param>
    /// <param name="message">The message to hash.</param>
    /// <returns>The Base64-encoded HMAC-SHA256 hash.</returns>
    public string CalculateAndEncode(string secret, string message) => Encode(Calculate(secret, message));
}
