using System;
using System.Runtime.InteropServices;

namespace OoBDev.Velleman.K8055;

/// <summary>
/// The state of every output. The board sets all outputs with one report, so a change to one channel is sent together with the last
/// known state of the others.
/// </summary>
/// <param name="DigitalOut">The eight digital outputs as a bitmask (channel 1 is bit 0).</param>
/// <param name="AnalogOut1">Analogue output 1 (0 to 255).</param>
/// <param name="AnalogOut2">Analogue output 2 (0 to 255).</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct K8055Outputs(byte DigitalOut, byte AnalogOut1, byte AnalogOut2)
{
    /// <summary>Returns the state with digital channel <paramref name="channel"/> (1 to 8) switched on or off.</summary>
    /// <param name="channel">The channel, 1 to 8.</param>
    /// <param name="on">Whether the channel is on.</param>
    public K8055Outputs WithDigital(int channel, bool on)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(channel, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(channel, 8);
        var mask = (byte)(1 << (channel - 1));
        return this with { DigitalOut = on ? (byte)(DigitalOut | mask) : (byte)(DigitalOut & ~mask) };
    }
}
