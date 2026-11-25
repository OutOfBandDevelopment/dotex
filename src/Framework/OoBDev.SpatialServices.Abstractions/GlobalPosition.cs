using System;

namespace OoBDev.SpatialServices;

/// <summary>A latitude and longitude in degrees.</summary>
public sealed record GlobalPosition
{
    /// <summary>Creates a position.</summary>
    /// <param name="latitude">Within -90..90.</param>
    /// <param name="longitude">Within -180..180.</param>
    /// <exception cref="ArgumentOutOfRangeException">A value is outside its range.</exception>
    public GlobalPosition(double latitude, double longitude)
    {
        if (double.IsNaN(latitude) || latitude is < -90 or > 90) throw new ArgumentOutOfRangeException(nameof(latitude), latitude, "Latitude must be within -90..90");
        if (double.IsNaN(longitude) || longitude is < -180 or > 180) throw new ArgumentOutOfRangeException(nameof(longitude), longitude, "Longitude must be within -180..180");
        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>Degrees north.</summary>
    public double Latitude { get; }

    /// <summary>Degrees east.</summary>
    public double Longitude { get; }
}
