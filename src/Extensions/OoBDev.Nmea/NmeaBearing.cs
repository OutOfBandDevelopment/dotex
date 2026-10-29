using System;
using System.Globalization;

namespace OoBDev.Nmea;

/// <summary>
/// A compass bearing in degrees true, normalised to at least 0 and less than 360.
/// </summary>
public readonly record struct NmeaBearing
{
    private static readonly string[] _abbreviations =
        ["N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW"];

    /// <summary>Creates a bearing; any finite angle is wrapped into the range 0 up to 360.</summary>
    /// <param name="degrees">The angle in degrees, clockwise from north.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="degrees"/> is NaN or infinite.</exception>
    public NmeaBearing(double degrees)
    {
        if (!double.IsFinite(degrees)) throw new ArgumentOutOfRangeException(nameof(degrees), degrees, "A bearing must be a finite number.");

        var wrapped = degrees % 360d;
        Degrees = wrapped < 0 ? wrapped + 360d : wrapped;
    }

    /// <summary>Gets the angle in degrees, at least 0 and less than 360.</summary>
    public double Degrees { get; }

    /// <summary>Gets the compass point this bearing falls in.</summary>
    public NmeaCompassPoint Point => (NmeaCompassPoint)((int)Math.Floor((Degrees + 11.25) / 22.5) % 16);

    /// <summary>Gets the short name of <see cref="Point"/>, for example <c>NNE</c>.</summary>
    public string Abbreviation => _abbreviations[(int)Point];

    /// <summary>Reads a bearing from a decimal text such as <c>084.4</c>.</summary>
    /// <param name="text">The text, in the invariant culture.</param>
    /// <param name="bearing">The bearing when the text is a finite number.</param>
    /// <returns><see langword="true"/> when <paramref name="bearing"/> is set.</returns>
    public static bool TryParse(string? text, out NmeaBearing bearing)
    {
        if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var degrees) && double.IsFinite(degrees))
        {
            bearing = new NmeaBearing(degrees);
            return true;
        }

        bearing = default;
        return false;
    }

    /// <summary>Formats as the point and the angle, for example <c>NNE (22.5000°)</c>.</summary>
    public override string ToString() =>
        string.Create(CultureInfo.InvariantCulture, $"{Abbreviation} ({Degrees:0.0000}°)");
}
