using System;

namespace OoBDev.Media.Metadata;

/// <summary>
/// The EXIF fields a catalog usually stores as columns. Any field that is missing or unreadable is <see langword="null"/>.
/// </summary>
/// <param name="Make">The camera maker, at most 100 characters.</param>
/// <param name="Model">The camera model, at most 100 characters.</param>
/// <param name="Orientation">The orientation tag (1 to 8 by the standard; other values are returned as stored).</param>
/// <param name="Taken">The capture time (original, else the file's date time), without a time zone.</param>
/// <param name="Latitude">Degrees north, -90 to 90.</param>
/// <param name="Longitude">Degrees east, -180 to 180.</param>
public sealed record ExifFields(string? Make, string? Model, int? Orientation, DateTime? Taken, double? Latitude, double? Longitude)
{
    /// <summary>A result with no field set.</summary>
    public static ExifFields Empty { get; } = new(null, null, null, null, null, null);
}
