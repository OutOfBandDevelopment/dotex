using System;
using System.Security.Cryptography;
using System.Text;

namespace OoBDev.System.Security.Cryptography;

/// <summary>
/// Default hash of input value.  Base64 encoded SHA512 Hash
/// </summary>
public class Sha512Hash : IHash
{
    /// <summary>
    /// Computes the default hash of the input value using SHA512.
    /// </summary>
    /// <param name="value">The input value to be hashed.</param>
    /// <returns>The Base64 encoded SHA512 hash of the input value.</returns>
    public virtual string GetHash(string value) =>
        Convert.ToBase64String(SHA512.HashData(Encoding.UTF8.GetBytes(value)));
}
