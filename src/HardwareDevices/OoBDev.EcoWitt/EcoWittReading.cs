using System;
using System.Collections.Generic;

namespace OoBDev.EcoWitt;

/// <summary>
/// One decoded EcoWitt "customized server" push. Values keep the units the gateway sends (Fahrenheit, inches, miles per hour,
/// inches of mercury); the Celsius and metric helpers convert. A value the push did not carry is <see langword="null"/>.
/// </summary>
public sealed record EcoWittReading
{
    private const double _mphToKph = 1.609344;
    private const double _inchToMm = 25.4;
    private const double _inHgToHpa = 33.8639;

    /// <summary>Gets the station type string, for example <c>GW1000_V1.7.3</c>.</summary>
    public string? StationType { get; init; }

    /// <summary>Gets the console model, for example <c>GW1100B</c>.</summary>
    public string? Model { get; init; }

    /// <summary>Gets the time the gateway stamped (UTC), or <see langword="null"/> when it sent <c>now</c> or nothing.</summary>
    public DateTimeOffset? Timestamp { get; init; }

    /// <summary>Gets the indoor temperature in degrees Fahrenheit.</summary>
    public double? IndoorTemperatureF { get; init; }

    /// <summary>Gets the indoor relative humidity in percent.</summary>
    public double? IndoorHumidity { get; init; }

    /// <summary>Gets the outdoor temperature in degrees Fahrenheit.</summary>
    public double? OutdoorTemperatureF { get; init; }

    /// <summary>Gets the outdoor relative humidity in percent.</summary>
    public double? OutdoorHumidity { get; init; }

    /// <summary>Gets the sea-level (relative) pressure in inches of mercury.</summary>
    public double? RelativePressureInHg { get; init; }

    /// <summary>Gets the station (absolute) pressure in inches of mercury.</summary>
    public double? AbsolutePressureInHg { get; init; }

    /// <summary>Gets the wind direction in degrees.</summary>
    public double? WindDirectionDegrees { get; init; }

    /// <summary>Gets the wind speed in miles per hour.</summary>
    public double? WindSpeedMph { get; init; }

    /// <summary>Gets the wind gust in miles per hour.</summary>
    public double? WindGustMph { get; init; }

    /// <summary>Gets the rain rate in inches per hour.</summary>
    public double? RainRateInPerHour { get; init; }

    /// <summary>Gets the rain since midnight in inches.</summary>
    public double? DailyRainIn { get; init; }

    /// <summary>Gets the rain this week in inches.</summary>
    public double? WeeklyRainIn { get; init; }

    /// <summary>Gets the rain this month in inches.</summary>
    public double? MonthlyRainIn { get; init; }

    /// <summary>Gets the rain this year in inches.</summary>
    public double? YearlyRainIn { get; init; }

    /// <summary>Gets the solar radiation in watts per square metre.</summary>
    public double? SolarRadiationWm2 { get; init; }

    /// <summary>Gets the UV index.</summary>
    public double? UvIndex { get; init; }

    /// <summary>Gets the fields the decoder does not map (extra sensors, battery flags), by name, as sent.</summary>
    public IReadOnlyDictionary<string, string> Unrecognised { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>Gets the fields that could not be decoded, each as a short message; the rest of the push still decoded.</summary>
    public IReadOnlyList<string> Problems { get; init; } = [];

    /// <summary>Gets the indoor temperature in degrees Celsius.</summary>
    public double? IndoorTemperatureC => ToCelsius(IndoorTemperatureF);

    /// <summary>Gets the outdoor temperature in degrees Celsius.</summary>
    public double? OutdoorTemperatureC => ToCelsius(OutdoorTemperatureF);

    /// <summary>Gets the wind speed in kilometres per hour.</summary>
    public double? WindSpeedKph => WindSpeedMph * _mphToKph;

    /// <summary>Gets the wind gust in kilometres per hour.</summary>
    public double? WindGustKph => WindGustMph * _mphToKph;

    /// <summary>Gets the daily rain in millimetres.</summary>
    public double? DailyRainMm => DailyRainIn * _inchToMm;

    /// <summary>Gets the sea-level pressure in hectopascals.</summary>
    public double? RelativePressureHpa => RelativePressureInHg * _inHgToHpa;

    private static double? ToCelsius(double? fahrenheit) => (fahrenheit - 32) * 5 / 9;
}
