using OoBDev.System.IO.Messages;
using System;

namespace OoBDev.Velleman.K8055;

/// <summary>
/// Encodes a <see cref="K8055Command"/> as the 9-byte write: the HID report id <c>0x00</c>, the command, then for
/// <see cref="K8055CommandKind.SetOutputs"/> the digital bitmask and both analogue bytes. The last four bytes (duration and debounce,
/// unconfirmed) stay zero, so outputs are a static level and not a pulse.
/// </summary>
public sealed class K8055MessageEncoder : IMessageEncoder<K8055Command>
{
    /// <inheritdoc/>
    public ReadOnlyMemory<byte> Encode(ref K8055Command request)
    {
        var report = new byte[K8055Reading.Length];
        report[1] = (byte)request.Kind;
        if (request.Kind == K8055CommandKind.SetOutputs)
        {
            report[2] = request.Outputs.DigitalOut;
            report[3] = request.Outputs.AnalogOut1;
            report[4] = request.Outputs.AnalogOut2;
        }
        return report;
    }
}
