using System;
using System.Linq;

namespace OoBDev.System.Cryptography;

/// <summary>
/// Implements the Caesar cipher, a classic substitution cipher where each letter is shifted by a fixed number of positions in the alphabet.
/// See https://en.wikipedia.org/wiki/Caesar_cipher for more information.
/// </summary>
/// <remarks>
/// WARNING: This is a classic cipher for educational purposes only. It provides no security and should never be used for protecting sensitive data.
/// </remarks>
public class Caesar
{
    /// <summary>
    /// Encodes a string using the Caesar cipher with the offset determined by the code character.
    /// Only alphabetic characters are shifted; other characters remain unchanged.
    /// </summary>
    /// <param name="input">The string to encode.</param>
    /// <param name="code">The code character ('A'-'Z' or 'a'-'z') that determines the shift offset.</param>
    /// <returns>The encoded string, or an empty string if input is null.</returns>
    public string Encode(string input, char code) =>
        (GetOffset(code), input) switch
        {
            (_, null) => "",
            (int offset, _) => new string([.. input.Select(c => Encode(c, offset))])
        };

    /// <summary>
    /// Decodes a string that was encoded using the Caesar cipher with the offset determined by the code character.
    /// Only alphabetic characters are shifted; other characters remain unchanged.
    /// </summary>
    /// <param name="input">The string to decode.</param>
    /// <param name="code">The code character ('A'-'Z' or 'a'-'z') that determines the shift offset.</param>
    /// <returns>The decoded string, or an empty string if input is null.</returns>
    public string Decode(string input, char code) =>
        (GetOffset(code), input) switch
        {
            (_, null) => "",
            (int offset, _) => new string([.. input.Select(c => Decode(c, offset))])
        };

    /// <summary>
    /// Encodes a single character using the Caesar cipher with the offset determined by the code character.
    /// </summary>
    /// <param name="input">The character to encode.</param>
    /// <param name="code">The code character ('A'-'Z' or 'a'-'z') that determines the shift offset.</param>
    /// <returns>The encoded character.</returns>
    public char Encode(char input, char code) => Encode(input, GetOffset(code));

    /// <summary>
    /// Decodes a single character that was encoded using the Caesar cipher with the offset determined by the code character.
    /// </summary>
    /// <param name="input">The character to decode.</param>
    /// <param name="code">The code character ('A'-'Z' or 'a'-'z') that determines the shift offset.</param>
    /// <returns>The decoded character.</returns>
    public char Decode(char input, char code) => Decode(input, GetOffset(code));

    /// <summary>
    /// Encodes a single character using the Caesar cipher with the specified offset.
    /// Only alphabetic characters are shifted; other characters remain unchanged.
    /// </summary>
    /// <param name="input">The character to encode.</param>
    /// <param name="offset">The number of positions to shift (0-25).</param>
    /// <returns>The encoded character.</returns>
    public char Encode(char input, int offset) =>
        input switch
        {
            >= 'A' and <= 'Z' => (char)('A' + (input - 'A' + offset) % 26),
            >= 'a' and <= 'z' => (char)('a' + (input - 'a' + offset) % 26),
            _ => input
        };

    /// <summary>
    /// Decodes a single character that was encoded using the Caesar cipher with the specified offset.
    /// Only alphabetic characters are shifted; other characters remain unchanged.
    /// </summary>
    /// <param name="input">The character to decode.</param>
    /// <param name="offset">The number of positions to shift back (0-25).</param>
    /// <returns>The decoded character.</returns>
    public char Decode(char input, int offset) =>
        input switch
        {
            >= 'A' and <= 'Z' => (char)('A' + (input + 26 - 'A' - offset) % 26),
            >= 'a' and <= 'z' => (char)('a' + (input + 26 - 'a' - offset) % 26),
            _ => input
        };

    /// <summary>
    /// Converts a code character to its corresponding offset value (0-25).
    /// </summary>
    /// <param name="code">The code character ('A'-'Z' or 'a'-'z').</param>
    /// <returns>The offset value (0 for 'A'/'a', 1 for 'B'/'b', etc.).</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the code character is not between 'A' and 'Z' or 'a' and 'z'.</exception>
    public int GetOffset(char code) => code switch
    {
        >= 'A' and <= 'Z' => code - 'A',
        >= 'a' and <= 'z' => code - 'a',
        _ => throw new ArgumentOutOfRangeException(nameof(code), "\"code\" must be between 'A' and 'Z'")
    };
}
