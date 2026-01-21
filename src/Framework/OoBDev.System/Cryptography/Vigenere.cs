using System.Linq;

namespace OoBDev.System.Cryptography;

/// <summary>
/// Implements the Vigenère cipher, a polyalphabetic substitution cipher that uses a keyword to shift letters by varying amounts.
/// See https://en.wikipedia.org/wiki/Vigen%C3%A8re_cipher for more information.
/// </summary>
/// <remarks>
/// WARNING: This is a classic cipher for educational purposes only. It provides no security and should never be used for protecting sensitive data.
/// </remarks>
public class Vigenere : Caesar
{
    /// <summary>
    /// Encodes a string using the Vigenère cipher with the specified keyword.
    /// The keyword is repeated to match the input length, and each character is shifted by the corresponding keyword character's offset.
    /// </summary>
    /// <param name="input">The string to encode.</param>
    /// <param name="code">The keyword that determines the shifting pattern.</param>
    /// <returns>The encoded string, or an empty string if input is null.</returns>
    public string Encode(string input, string code) =>
        (input, BuildKey(input.Length, code)) switch
        {
            (null, _) => "",
            (string, string key) => new string([.. input.Zip(key).Select(item => Encode(item.First, item.Second))])
        };

    /// <summary>
    /// Decodes a string that was encoded using the Vigenère cipher with the specified keyword.
    /// The keyword is repeated to match the input length, and each character is shifted back by the corresponding keyword character's offset.
    /// </summary>
    /// <param name="input">The string to decode.</param>
    /// <param name="code">The keyword that determines the shifting pattern.</param>
    /// <returns>The decoded string, or an empty string if input is null.</returns>
    public string Decode(string input, string code) =>
        (input, BuildKey(input.Length, code)) switch
        {
            (null, _) => "",
            (string, string key) => new string([.. input.Zip(key).Select(item => Decode(item.First, item.Second))])
        };

    /// <summary>
    /// Builds a repeating key string by repeating the code keyword to match the specified length.
    /// Non-letter characters are removed from the code. If the code is empty or null, generates a sequential alphabetic key (A, B, C, ...).
    /// </summary>
    /// <param name="length">The desired length of the key.</param>
    /// <param name="code">The keyword to repeat (null or empty generates a sequential alphabetic key).</param>
    /// <returns>A key string of the specified length.</returns>
    public string BuildKey(int length, string? code)
    {
        code = new string([.. (code ?? string.Empty).Where(char.IsLetter)]);
        return string.IsNullOrWhiteSpace(code)
            ? new string([.. Enumerable.Range(0, length).Select(i => (char)('A' + i % 26))])
            : string.Join("", Enumerable.Range(0, length / code.Length + 1).Select(_ => code))[..length];
    }
}
