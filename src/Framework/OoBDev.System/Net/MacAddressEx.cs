using BinaryDataDecoders.Net;
using System.Text.RegularExpressions;

namespace OoBDev.System.Net;

/// <summary>
/// Provides utility methods for validating and parsing MAC (Media Access Control) addresses.
/// </summary>
public static class MacAddressEx
{
    /// <summary>
    /// Determines whether the specified string is a valid MAC address in the format XX:XX:XX:XX:XX:XX or XX-XX-XX-XX-XX-XX.
    /// </summary>
    /// <param name="macAddress">The string to validate as a MAC address.</param>
    /// <returns>true if the string is a valid MAC address; otherwise, false.</returns>
    public static bool IsValid(string macAddress)
    {
        var macPattern = new Regex("^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$", RegexOptions.Compiled);
        return macPattern.IsMatch(macAddress);
    }

    /// <summary>
    /// Parses a MAC address string into a byte array.
    /// </summary>
    /// <param name="macAddress">The MAC address string to parse (format: XX:XX:XX:XX:XX:XX or XX-XX-XX-XX-XX-XX).</param>
    /// <returns>A byte array containing the 6-byte MAC address.</returns>
    /// <exception cref="InvalidMacAddressException">Thrown when the MAC address format is invalid.</exception>
    public static byte[] Parse(string macAddress)
    {
        InvalidMacAddressException.Check(macAddress);
        var macBuffer = ConvertEx.FromHexString(macAddress.Replace("-", "").Replace(":", ""));
        return macBuffer;
    }

    /// <summary>
    /// Attempts to parse a MAC address string into a byte array.
    /// </summary>
    /// <param name="macAddress">The MAC address string to parse.</param>
    /// <param name="macBuffer">When this method returns, contains the parsed MAC address byte array if parsing succeeded, or an empty array if parsing failed.</param>
    /// <returns>true if the MAC address was successfully parsed; otherwise, false.</returns>
    public static bool TryParse(string macAddress, out byte[] macBuffer)
    {
        if (IsValid(macAddress))
        {
            macBuffer = ConvertEx.FromHexString(macAddress.Replace("-", "").Replace(":", ""));
            return true;
        }
        else
        {
            macBuffer = [];
            return false;
        }
    }
}
