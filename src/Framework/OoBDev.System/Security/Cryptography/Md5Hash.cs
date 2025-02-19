using System;
using System.Security.Cryptography;
using System.Text;

namespace OoBDev.System.Security.Cryptography;

/// <summary>
/// Default hash of input value.  Base64 encoded MD5 Hash
/// </summary>
public class Md5Hash : IHash
{
    /// <summary>
    /// Computes the default hash of the input value using MD5.
    /// </summary>
    /// <param name="value">The input value to be hashed.</param>
    /// <returns>The Base64 encoded MD5 hash of the input value.</returns>
    public virtual string GetHash(string value) =>
        Convert.ToBase64String(MD5.HashData(Encoding.UTF8.GetBytes(value)));
}
