using System;

namespace OoBDev.Velleman.K8055;

/// <summary>
/// The five digital inputs of the K8055 as bits of the input byte. The bit order is not the numeric order (I3 is bit 0); the names
/// come from the BinaryDataDecoders implementation and have not been confirmed by wiring each input on the board.
/// </summary>
[Flags]
public enum K8055DigitalInputs : byte
{
    /// <summary>No input is high.</summary>
    None = 0,

    /// <summary>Digital input 3.</summary>
    I3 = 0x01,

    /// <summary>Digital input 1.</summary>
    I1 = 0x10,

    /// <summary>Digital input 2.</summary>
    I2 = 0x20,

    /// <summary>Digital input 4.</summary>
    I4 = 0x40,

    /// <summary>Digital input 5.</summary>
    I5 = 0x80,
}
