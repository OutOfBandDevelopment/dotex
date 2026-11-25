namespace OoBDev.Nmea;

/// <summary>
/// The GSA fix type.
/// </summary>
public enum NmeaFixType
{
    /// <summary>A code this package does not name.</summary>
    Unknown = 0,

    /// <summary>No fix (1).</summary>
    NoFix = 1,

    /// <summary>Two dimensional fix (2).</summary>
    Fix2D = 2,

    /// <summary>Three dimensional fix (3).</summary>
    Fix3D = 3,
}
