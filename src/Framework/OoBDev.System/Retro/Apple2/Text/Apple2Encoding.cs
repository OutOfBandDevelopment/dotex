using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace OoBDev.System.Retro.Apple2.Text;

/// <summary>
/// Represents Apple ][ encoding of Unicode characters.
/// </summary>
/// <remarks>
/// Normal text has bit 7 set and maps to the ASCII character with bit 7 cleared. Inverse and flashing text
/// have bit 7 clear; those bytes map to the private-use character U+E000 plus the byte value, so every
/// byte round-trips. Characters other than ASCII and that private-use range encode as '?'.
/// </remarks>
public class Apple2Encoding : Encoding
{
    /// <summary>
    /// First private-use character used for bytes with bit 7 clear (inverse and flashing text).
    /// </summary>
    public const char AttributeBase = '';

    /// <summary>
    /// Get common instance of Apple2Encoding
    /// </summary>
    public static Apple2Encoding Apple2 { get; } = new Apple2Encoding();

    /// <summary>
    /// Description for email tags
    /// </summary>
    [ExcludeFromCodeCoverage]
    public override string BodyName => "Apple ][";
    /// <summary>
    /// Name for encoding
    /// </summary>
    [ExcludeFromCodeCoverage]
    public override string EncodingName => "Apple ][";

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override bool IsSingleByte => true;

    /// <inheritdoc />
    public override int GetByteCount(char[] chars, int index, int count) => count;

    /// <inheritdoc />
    public override int GetCharCount(byte[] bytes, int index, int count) => count;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override int GetMaxByteCount(int charCount) => charCount;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override int GetMaxCharCount(int byteCount) => byteCount;

    /// <inheritdoc />
    public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
    {
        for (var i = 0; i < charCount; i++)
        {
            bytes[byteIndex + i] = ToByte(chars[charIndex + i]);
        }
        return charCount;
    }

    /// <inheritdoc />
    public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
    {
        for (var i = 0; i < byteCount; i++)
        {
            chars[charIndex + i] = ToChar(bytes[byteIndex + i]);
        }
        return byteCount;
    }

    private static byte ToByte(char c) => c switch
    {
        < '\u0080' => (byte)(c | 0x80),
        >= AttributeBase and < (char)(AttributeBase + 0x80) => (byte)(c - AttributeBase),
        _ => (byte)('?' | 0x80),
    };

    private static char ToChar(byte b) => (b & 0x80) != 0 ? (char)(b & 0x7f) : (char)(AttributeBase + b);
}
