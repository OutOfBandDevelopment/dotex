using System.Collections.Generic;

namespace OoBDev.EcoWitt;

/// <summary>
/// The sensor values of one local-API live data response, in the metric units the gateway sends. A value the response did not
/// carry is <see langword="null"/>.
/// </summary>
public sealed record EcoWittLiveReading
{
    /// <summary>Gets the indoor temperature in degrees Celsius.</summary>
    public double? IndoorTemperatureC { get; init; }

    /// <summary>Gets the outdoor temperature in degrees Celsius.</summary>
    public double? OutdoorTemperatureC { get; init; }

    /// <summary>Gets the dew point in degrees Celsius.</summary>
    public double? DewPointC { get; init; }

    /// <summary>Gets the wind chill in degrees Celsius.</summary>
    public double? WindChillC { get; init; }

    /// <summary>Gets the heat index in degrees Celsius.</summary>
    public double? HeatIndexC { get; init; }

    /// <summary>Gets the indoor relative humidity in percent.</summary>
    public double? IndoorHumidity { get; init; }

    /// <summary>Gets the outdoor relative humidity in percent.</summary>
    public double? OutdoorHumidity { get; init; }

    /// <summary>Gets the station (absolute) pressure in hectopascals.</summary>
    public double? AbsolutePressureHpa { get; init; }

    /// <summary>Gets the sea-level (relative) pressure in hectopascals.</summary>
    public double? RelativePressureHpa { get; init; }

    /// <summary>Gets the wind direction in degrees.</summary>
    public double? WindDirectionDegrees { get; init; }

    /// <summary>Gets the wind speed in metres per second.</summary>
    public double? WindSpeedMs { get; init; }

    /// <summary>Gets the wind gust in metres per second.</summary>
    public double? WindGustMs { get; init; }

    /// <summary>Gets the rain of the current rain event in millimetres.</summary>
    public double? RainEventMm { get; init; }

    /// <summary>Gets the rain rate in millimetres per hour.</summary>
    public double? RainRateMmPerHour { get; init; }

    /// <summary>Gets the rain of the last hour in millimetres.</summary>
    public double? RainHourMm { get; init; }

    /// <summary>Gets the rain today in millimetres.</summary>
    public double? RainDayMm { get; init; }

    /// <summary>Gets the rain this week in millimetres.</summary>
    public double? RainWeekMm { get; init; }

    /// <summary>Gets the rain this month in millimetres.</summary>
    public double? RainMonthMm { get; init; }

    /// <summary>Gets the rain this year in millimetres.</summary>
    public double? RainYearMm { get; init; }

    /// <summary>Gets the light level in lux.</summary>
    public double? LightLux { get; init; }

    /// <summary>Gets the ultraviolet radiation in microwatts per square centimetre.</summary>
    public double? UvMicrowattPerCm2 { get; init; }

    /// <summary>Gets the UV index.</summary>
    public double? UvIndex { get; init; }

    /// <summary>Gets the problems found while decoding, each as a short message; the values before the problem still decoded.</summary>
    public IReadOnlyList<string> Problems { get; init; } = [];
}
