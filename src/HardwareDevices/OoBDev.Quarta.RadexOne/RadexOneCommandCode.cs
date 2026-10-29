namespace OoBDev.Quarta.RadexOne;

/// <summary>
/// The command codes of the Radex One protocol, carried in the first two bytes of a packet's extension.
/// </summary>
public static class RadexOneCommandCode
{
    /// <summary>Read the ambient, accumulated and counts-per-minute values.</summary>
    public const ushort ReadData = 0x0800;

    /// <summary>Read the serial number and version text.</summary>
    public const ushort ReadSerialVersion = 0x0001;

    /// <summary>Read the alarm mode and threshold.</summary>
    public const ushort ReadSettings = 0x0801;

    /// <summary>Write the alarm mode and threshold. The device needs the request three times in a row to take effect.</summary>
    public const ushort WriteSettings = 0x0802;

    /// <summary>Reset the accumulated dose.</summary>
    public const ushort ResetAccumulated = 0x0803;
}
