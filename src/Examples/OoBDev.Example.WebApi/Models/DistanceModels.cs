namespace OoBDev.WebApi.Models;

/// <summary>Two coordinates to measure between.</summary>
/// <param name="FromLatitude">Start latitude, -90 to 90.</param>
/// <param name="FromLongitude">Start longitude, -180 to 180.</param>
/// <param name="ToLatitude">End latitude, -90 to 90.</param>
/// <param name="ToLongitude">End longitude, -180 to 180.</param>
public sealed record DistanceRequestModel(double FromLatitude, double FromLongitude, double ToLatitude, double ToLongitude);

/// <summary>The great-circle result.</summary>
/// <param name="Meters">Distance in metres.</param>
/// <param name="Kilometers">Distance in kilometres.</param>
/// <param name="Miles">Distance in statute miles.</param>
/// <param name="BearingDegrees">Initial compass bearing from the start, 0 to under 360.</param>
public sealed record DistanceResponseModel(double Meters, double Kilometers, double Miles, double BearingDegrees);
