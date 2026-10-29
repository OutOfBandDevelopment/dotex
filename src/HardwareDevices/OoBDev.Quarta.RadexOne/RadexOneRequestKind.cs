namespace OoBDev.Quarta.RadexOne;

/// <summary>
/// What a <see cref="RadexOneRequest"/> asks of the device.
/// </summary>
public enum RadexOneRequestKind
{
    /// <summary>Read the ambient, accumulated and counts-per-minute values.</summary>
    ReadData,

    /// <summary>Read the serial number and version text.</summary>
    ReadSerialVersion,

    /// <summary>Read the alarm settings.</summary>
    ReadSettings,

    /// <summary>Write the alarm settings. The device takes it only when sent three times in a row; that repetition is the caller's.</summary>
    WriteSettings,

    /// <summary>Reset the accumulated dose.</summary>
    ResetAccumulated,
}
