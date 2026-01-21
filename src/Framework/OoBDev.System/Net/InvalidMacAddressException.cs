using System;
using System.Diagnostics;

namespace OoBDev.System.Net;

/// <summary>
/// Exception thrown when a string is not a valid MAC (Media Access Control) address.
/// </summary>
/// <param name="macAddress">The invalid MAC address string that caused the exception.</param>
public class InvalidMacAddressException(string macAddress) : Exception(string.Format("\"{0}\" is not a valid MAC Address", macAddress))
{
    /// <summary>
    /// Gets the MAC address string that was invalid.
    /// </summary>
    public string MACAddress { get; } = macAddress;

    /// <summary>
    /// Validates that a string is a valid MAC address and throws an exception if it is not.
    /// </summary>
    /// <param name="macAddress">The string to validate as a MAC address.</param>
    /// <exception cref="InvalidMacAddressException">Thrown when the string is not a valid MAC address.</exception>
    [DebuggerNonUserCode]
    public static void Check(string macAddress)
    {
        if (!MacAddressEx.IsValid(macAddress))
            throw new InvalidMacAddressException(macAddress);
    }
}
