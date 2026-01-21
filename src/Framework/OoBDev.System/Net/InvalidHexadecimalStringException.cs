using BinaryDataDecoders.Net;
using System;
using System.Diagnostics;

namespace OoBDev.System.Net;

/// <summary>
/// Exception thrown when a string is not a valid hexadecimal representation.
/// </summary>
/// <param name="hexString">The invalid hexadecimal string that caused the exception.</param>
public class InvalidHexadecimalStringException(string hexString) : Exception(string.Format("\"{0}\" is not a valid Hexadecimal Number", hexString))
{
    /// <summary>
    /// Gets the hexadecimal string that was invalid.
    /// </summary>
    public string Hexadecimal { get; } = hexString;

    /// <summary>
    /// Validates that a string is a valid hexadecimal number and throws an exception if it is not.
    /// </summary>
    /// <param name="hexString">The string to validate as hexadecimal.</param>
    /// <exception cref="InvalidHexadecimalStringException">Thrown when the string is not a valid hexadecimal number.</exception>
    [DebuggerNonUserCode]
    public static void Check(string hexString)
    {
        if (!ConvertEx.IsHexString(hexString))
            throw new InvalidHexadecimalStringException(hexString);
    }
}
