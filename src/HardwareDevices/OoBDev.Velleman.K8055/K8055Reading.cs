using System.Runtime.InteropServices;

namespace OoBDev.Velleman.K8055;

/// <summary>
/// One input report from the board.
/// </summary>
/// <param name="DigitalInputsRaw">The digital input byte as sent; <see cref="DigitalInputs"/> names its bits.</param>
/// <param name="AnalogIn1">Analogue input 1 (0 to 255).</param>
/// <param name="AnalogIn2">Analogue input 2 (0 to 255).</param>
/// <param name="Counter1">Pulse counter 1.</param>
/// <param name="Counter2">Pulse counter 2.</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct K8055Reading(byte DigitalInputsRaw, byte AnalogIn1, byte AnalogIn2, ushort Counter1, ushort Counter2)
{
    /// <summary>The report length: a leading report id byte and the 8-byte payload.</summary>
    public const int Length = 9;

    /// <summary>The digital input byte with named bits.</summary>
    public K8055DigitalInputs DigitalInputs => (K8055DigitalInputs)DigitalInputsRaw;
}
