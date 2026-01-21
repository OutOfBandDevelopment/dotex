using System;

namespace OoBDev.System.IO.Ports;

/// <summary>
/// Provides extension methods for converting between OoBDev serial port types and System.IO.Ports types.
/// </summary>
public static class BridgeExtensions
{
    /// <summary>
    /// Converts an OoBDev <see cref="Parity"/> value to a <see cref="global::System.IO.Ports.Parity"/> value.
    /// </summary>
    /// <param name="parity">The OoBDev parity value to convert.</param>
    /// <returns>The corresponding System.IO.Ports parity value.</returns>
    /// <exception cref="ArgumentException">Thrown when the parity value is not recognized.</exception>
    public static global::System.IO.Ports.Parity AsSystem(this Parity parity) =>
        parity switch
        {
            Parity.None => global::System.IO.Ports.Parity.None,
            Parity.Even => global::System.IO.Ports.Parity.Even,
            Parity.Mark => global::System.IO.Ports.Parity.Mark,
            Parity.Odd => global::System.IO.Ports.Parity.Odd,
            Parity.Space => global::System.IO.Ports.Parity.Space,
            _ => throw new ArgumentException(nameof(parity))
        };

    /// <summary>
    /// Converts an OoBDev <see cref="StopBits"/> value to a <see cref="global::System.IO.Ports.StopBits"/> value.
    /// </summary>
    /// <param name="stopBits">The OoBDev stop bits value to convert.</param>
    /// <returns>The corresponding System.IO.Ports stop bits value.</returns>
    /// <exception cref="ArgumentException">Thrown when the stop bits value is not recognized.</exception>
    public static global::System.IO.Ports.StopBits AsSystem(this StopBits stopBits) =>
        stopBits switch
        {
            StopBits.None => global::System.IO.Ports.StopBits.None,
            StopBits.One => global::System.IO.Ports.StopBits.One,
            StopBits.OnePointFive => global::System.IO.Ports.StopBits.OnePointFive,
            StopBits.Two => global::System.IO.Ports.StopBits.Two,
            _ => throw new ArgumentException(nameof(stopBits))
        };
}
