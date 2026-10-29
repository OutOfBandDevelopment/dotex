namespace OoBDev.Nmea;

/// <summary>
/// The 16 points of the compass rose, clockwise from north; each covers 22.5 degrees centred on its bearing.
/// </summary>
public enum NmeaCompassPoint
{
    /// <summary>N, 0 degrees.</summary>
    North = 0,
    /// <summary>NNE, 22.5 degrees.</summary>
    NorthNorthEast = 1,
    /// <summary>NE, 45 degrees.</summary>
    NorthEast = 2,
    /// <summary>ENE, 67.5 degrees.</summary>
    EastNorthEast = 3,
    /// <summary>E, 90 degrees.</summary>
    East = 4,
    /// <summary>ESE, 112.5 degrees.</summary>
    EastSouthEast = 5,
    /// <summary>SE, 135 degrees.</summary>
    SouthEast = 6,
    /// <summary>SSE, 157.5 degrees.</summary>
    SouthSouthEast = 7,
    /// <summary>S, 180 degrees.</summary>
    South = 8,
    /// <summary>SSW, 202.5 degrees.</summary>
    SouthSouthWest = 9,
    /// <summary>SW, 225 degrees.</summary>
    SouthWest = 10,
    /// <summary>WSW, 247.5 degrees.</summary>
    WestSouthWest = 11,
    /// <summary>W, 270 degrees.</summary>
    West = 12,
    /// <summary>WNW, 292.5 degrees.</summary>
    WestNorthWest = 13,
    /// <summary>NW, 315 degrees.</summary>
    NorthWest = 14,
    /// <summary>NNW, 337.5 degrees.</summary>
    NorthNorthWest = 15,
}
