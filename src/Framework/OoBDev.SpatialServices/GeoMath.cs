using System;
namespace OoBDev.SpatialServices;

/// <summary>Great-circle calculations on a spherical Earth (mean radius); accurate to about 0.5 percent.</summary>
public static class GeoMath
{
    /// <summary>Mean Earth radius in metres.</summary>
    public const double EarthRadiusMeters = 6_371_008.8;

    /// <summary>Haversine distance between two positions, in metres.</summary>
    /// <param name="from">Start.</param>
    /// <param name="to">End.</param>
    public static double DistanceMeters(GlobalPosition from, GlobalPosition to)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);
        var lat1 = ToRadians(from.Latitude);
        var lat2 = ToRadians(to.Latitude);
        var dLat = lat2 - lat1;
        var dLon = ToRadians(to.Longitude - from.Longitude);
        var a = Math.Pow(Math.Sin(dLat / 2), 2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dLon / 2), 2);
        return 2 * EarthRadiusMeters * Math.Asin(Math.Min(1, Math.Sqrt(a)));
    }

    /// <summary>Initial compass bearing from one position to another, in degrees from north (0 to under 360).</summary>
    /// <param name="from">Start.</param>
    /// <param name="to">End.</param>
    public static double BearingDegrees(GlobalPosition from, GlobalPosition to)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);
        var lat1 = ToRadians(from.Latitude);
        var lat2 = ToRadians(to.Latitude);
        var dLon = ToRadians(to.Longitude - from.Longitude);
        var y = Math.Sin(dLon) * Math.Cos(lat2);
        var x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);
        return (ToDegrees(Math.Atan2(y, x)) + 360) % 360;
    }

    /// <summary>The position reached by travelling a distance along a bearing.</summary>
    /// <param name="from">Start.</param>
    /// <param name="bearingDegrees">Compass bearing in degrees.</param>
    /// <param name="meters">Distance in metres.</param>
    public static GlobalPosition Destination(GlobalPosition from, double bearingDegrees, double meters)
    {
        ArgumentNullException.ThrowIfNull(from);
        var delta = meters / EarthRadiusMeters;
        var theta = ToRadians(bearingDegrees);
        var lat1 = ToRadians(from.Latitude);
        var lon1 = ToRadians(from.Longitude);
        var lat2 = Math.Asin(Math.Sin(lat1) * Math.Cos(delta) + Math.Cos(lat1) * Math.Sin(delta) * Math.Cos(theta));
        var lon2 = lon1 + Math.Atan2(Math.Sin(theta) * Math.Sin(delta) * Math.Cos(lat1), Math.Cos(delta) - Math.Sin(lat1) * Math.Sin(lat2));
        var lon = (ToDegrees(lon2) + 540) % 360 - 180;
        return new GlobalPosition(Math.Clamp(ToDegrees(lat2), -90, 90), lon);
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;

    private static double ToDegrees(double radians) => radians * 180 / Math.PI;
}
