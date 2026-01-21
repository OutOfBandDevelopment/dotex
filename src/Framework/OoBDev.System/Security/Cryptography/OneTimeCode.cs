using OoBDev.System.Codecs;
using System;
using System.Security.Cryptography;
using System.Text;

namespace OoBDev.System.Security.Cryptography;

/// <summary>
/// Provides functionality for generating and validating one-time passwords (OTP) using TOTP and HOTP algorithms.
/// </summary>
public class OneTimeCode
{
    /// <summary>
    /// The Unix epoch timestamp (January 1, 1970, 00:00:00 UTC).
    /// </summary>
    public static readonly DateTime UNIX_EPOCH = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Base32 codec for encoding and decoding secrets.
    /// </summary>
    public static readonly Base32Codec Base32Encoding = new();

    /// <summary>
    /// Gets the current time-based counter value (30-second intervals since Unix epoch).
    /// </summary>
    /// <returns>The current counter value.</returns>
    public long GetCurrentCounter()
    {
        var counter = (long)(DateTime.UtcNow - UNIX_EPOCH).TotalSeconds / 30;
        return counter;
    }

    /// <summary>
    /// Generates a one-time password token for the specified secret and iteration number.
    /// </summary>
    /// <param name="secret">The Base32-encoded secret key.</param>
    /// <param name="iterationNumber">The counter value (time step for TOTP, counter for HOTP).</param>
    /// <param name="digits">The number of digits in the generated token (default is 6).</param>
    /// <returns>The generated OTP token.</returns>
    public string GenerateToken(string secret, long iterationNumber, int digits = 6)
    {
        var counter = BitConverter.GetBytes(iterationNumber);

        if (BitConverter.IsLittleEndian)
            Array.Reverse(counter);

        var key = GetKey(secret);

        using var hmac = new HMACSHA1(key);
        var hash = hmac.ComputeHash(counter);

        var offset = hash[hash.Length - 1] & 0xf;

        var binary =
            (hash[offset] & 0x7f) << 24
            | (hash[offset + 1] & 0xff) << 16
            | (hash[offset + 2] & 0xff) << 8
            | hash[offset + 3] & 0xff;

        var password = binary % (int)global::System.Math.Pow(10, digits); // 6 digits

        var result = password.ToString(new string('0', digits));

        return result;
    }

    /// <summary>
    /// Gets a one-time password token for the specified secret.
    /// </summary>
    /// <param name="secret">The Base32-encoded secret key.</param>
    /// <param name="counter">Optional counter value; if null, uses the current time-based counter.</param>
    /// <returns>The generated OTP token.</returns>
    public string GetToken(string secret, long? counter = null) => GenerateToken(secret, counter ?? GetCurrentCounter());

    /// <summary>
    /// Validates a one-time password token against the specified secret.
    /// </summary>
    /// <param name="secret">The Base32-encoded secret key.</param>
    /// <param name="token">The token to validate.</param>
    /// <param name="checkAdjacentIntervals">Number of adjacent time intervals to check (default is 1).</param>
    /// <returns>True if the token is valid; otherwise, false.</returns>
    public bool IsValid(string secret, string token, int checkAdjacentIntervals = 1)
    {
        if (token == GetToken(secret))
            return true;

        if (checkAdjacentIntervals < 1)
            checkAdjacentIntervals = 1;

        for (var i = 1; i <= checkAdjacentIntervals; i++)
        {
            string check;
            if (token == (check = GetToken(secret, GetCurrentCounter() + i)))
            {
                return true;
            }
            if (token == (check = GetToken(secret, GetCurrentCounter() - i)))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Generates a random Base32-encoded secret key for OTP authentication.
    /// </summary>
    /// <returns>The generated secret key.</returns>
    public string GenerateSecret()
    {
        var buffer = RandomNumberGenerator.GetBytes(9);
        var secret = Convert.ToBase64String(buffer)[..10].Replace('/', '0').Replace('+', '1');
        var encoded = Base32Encoding.Encode(Encoding.ASCII.GetBytes(secret));
        return encoded;
    }

    /// <summary>
    /// Decodes a Base32-encoded secret into a byte array key.
    /// </summary>
    /// <param name="secret">The Base32-encoded secret.</param>
    /// <returns>The decoded key as a byte array.</returns>
    public byte[] GetKey(string secret)
    {
        var decoded = Base32Encoding.Decode(secret);
        return decoded;
    }

    /// <summary>
    /// Generates an OTP authentication URI for QR code generation.
    /// </summary>
    /// <param name="secret">The Base32-encoded secret key.</param>
    /// <param name="issuer">The name of the service issuing the OTP.</param>
    /// <param name="account">Optional account identifier.</param>
    /// <param name="type">The OTP type (TOTP or HOTP).</param>
    /// <returns>The OTP authentication URI.</returns>
    public string GetUri(string secret, string issuer, string? account = null, Types type = Types.TOTP) =>
        $"otpauth://{type.ToString().ToLower()}/{issuer}{(!string.IsNullOrWhiteSpace(account) ? ":" + account : null)}?secret={secret}&issuer={issuer}";

    /// <summary>
    /// Specifies the type of one-time password algorithm.
    /// </summary>
    public enum Types
    {
        /// <summary>
        /// HMAC-based One-Time Password (counter-based).
        /// </summary>
        HOTP,

        /// <summary>
        /// Time-based One-Time Password.
        /// </summary>
        TOTP,
    }
}
