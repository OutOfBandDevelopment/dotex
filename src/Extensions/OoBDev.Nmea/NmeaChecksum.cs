namespace OoBDev.Nmea;

/// <summary>
/// The result of checking the <c>*hh</c> checksum of a sentence.
/// </summary>
public enum NmeaChecksum
{
    /// <summary>The sentence carries no checksum.</summary>
    Absent,

    /// <summary>The XOR of the characters between <c>$</c> and <c>*</c> matches.</summary>
    Valid,

    /// <summary>The checksum does not match the sentence.</summary>
    Mismatch,
}
