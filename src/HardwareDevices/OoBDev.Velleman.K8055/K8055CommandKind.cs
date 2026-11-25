namespace OoBDev.Velleman.K8055;

/// <summary>
/// The commands the board understands.
/// </summary>
public enum K8055CommandKind : byte
{
    /// <summary>Reset pulse counter 1.</summary>
    ResetCounter1 = 0x03,

    /// <summary>Reset pulse counter 2.</summary>
    ResetCounter2 = 0x04,

    /// <summary>Set the digital and analogue outputs together.</summary>
    SetOutputs = 0x05,
}
