using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace OoBDev.System;

/// <summary>
/// Provides extension methods for string hashing operations.
/// </summary>
public static class StringEx
{
    /// <summary>
    /// Computes the SHA-256 hash of a string and returns it as a hexadecimal string.
    /// </summary>
    /// <param name="text">The string to hash.</param>
    /// <returns>The SHA-256 hash as a hexadecimal string, or null if the input is null or whitespace.</returns>
    public static string? AsSha256(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        using var hashstring = SHA256.Create();
        return text.AsHash(hashstring);
    }

    /// <summary>
    /// Computes the MD5 hash of a string and returns it as a hexadecimal string.
    /// </summary>
    /// <param name="text">The string to hash.</param>
    /// <returns>The MD5 hash as a hexadecimal string, or null if the input is null or whitespace.</returns>
    public static string? AsMd5(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        using var hashstring = MD5.Create();
        return text.AsHash(hashstring);
    }

    /// <summary>
    /// Computes the hash of a string using the specified hash algorithm and returns it as a hexadecimal string.
    /// </summary>
    /// <param name="text">The string to hash.</param>
    /// <param name="hashAlgorithm">The hash algorithm to use for computing the hash.</param>
    /// <returns>The hash as a hexadecimal string, or null if the input is null or whitespace.</returns>
    public static string? AsHash(this string text, HashAlgorithm hashAlgorithm)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var buffer = Encoding.UTF8.GetBytes(text);
        var hash = hashAlgorithm.ComputeHash(buffer);
        var result = hash.Aggregate(new StringBuilder(), (sb, v) => sb.AppendFormat("{0:x2}", v));
        return result.ToString();
    }
}
