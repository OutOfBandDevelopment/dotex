namespace OoBDev.System.Security.Cryptography;

/// <summary>
/// Simplified hash generator
/// </summary>
public interface IHash
{
    /// <summary>
    /// cryptographic has for input value
    /// </summary>
    /// <param name="value">value to hash</param>
    /// <returns>hash input</returns>
    string GetHash(string value);
}
