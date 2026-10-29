namespace OoBDev.Nmea;

/// <summary>
/// The GGA fix quality indicator.
/// </summary>
public enum NmeaFixQuality
{
    /// <summary>No fix (0, or blank).</summary>
    NoFix = 0,

    /// <summary>GPS fix (1).</summary>
    Gps = 1,

    /// <summary>Differential GPS fix (2).</summary>
    Dgps = 2,

    /// <summary>PPS fix (3).</summary>
    Pps = 3,

    /// <summary>Real time kinematic, fixed (4).</summary>
    RtkFixed = 4,

    /// <summary>Real time kinematic, float (5).</summary>
    RtkFloat = 5,

    /// <summary>Estimated, dead reckoning (6).</summary>
    Estimated = 6,

    /// <summary>A code this package does not name.</summary>
    Unknown = -1,
}
