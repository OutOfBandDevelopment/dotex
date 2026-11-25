using System;
using System.Collections.Generic;
using System.Globalization;

namespace OoBDev.EcoWitt;

/// <summary>
/// Decodes the form-encoded body an EcoWitt gateway posts to a "customized server" (Ecowitt protocol). Pure: no I/O. The
/// <c>PASSKEY</c> field, which identifies the gateway to the cloud, is dropped and never kept.
/// </summary>
public sealed class EcoWittPushDecoder
{
    private static readonly string[] _timestampFormats = ["yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd'T'HH:mm:ss"];

    private static readonly HashSet<string> _numeric = new(StringComparer.Ordinal)
    {
        "tempinf", "humidityin", "tempf", "humidity", "baromrelin", "baromabsin", "winddir", "windspeedmph", "windgustmph",
        "rainratein", "dailyrainin", "weeklyrainin", "monthlyrainin", "yearlyrainin", "solarradiation", "uv",
    };

    /// <summary>
    /// Decodes one push body.
    /// </summary>
    /// <param name="body">The <c>application/x-www-form-urlencoded</c> body, for example <c>tempf=71.6&amp;humidity=48</c>.</param>
    /// <returns>The reading. An unknown field lands in <see cref="EcoWittReading.Unrecognised"/>, a bad one in <see cref="EcoWittReading.Problems"/>.</returns>
    public EcoWittReading Decode(string body)
    {
        ArgumentNullException.ThrowIfNull(body);
        var unrecognised = new Dictionary<string, string>(StringComparer.Ordinal);
        var problems = new List<string>();
        var numbers = new Dictionary<string, double>(StringComparer.Ordinal);
        string? stationType = null;
        string? model = null;
        DateTimeOffset? timestamp = null;

        foreach (var pair in body.Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var split = pair.IndexOf('=', StringComparison.Ordinal);
            if (split <= 0)
            {
                problems.Add($"'{pair}' is not a name=value pair");
                continue;
            }

            var name = Unescape(pair[..split]);
            var value = Unescape(pair[(split + 1)..]);
            switch (name)
            {
                case "PASSKEY":
                    break;
                case "stationtype":
                    stationType = value;
                    break;
                case "model":
                    model = value;
                    break;
                case "dateutc":
                    timestamp = ParseTimestamp(value, problems);
                    break;
                default:
                    if (!_numeric.Contains(name))
                    {
                        unrecognised[name] = value;
                    }
                    else if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
                    {
                        numbers[name] = number;
                    }
                    else
                    {
                        problems.Add($"{name}='{value}' is not a number");
                    }

                    break;
            }
        }

        return ToReading(numbers, stationType, model, timestamp, unrecognised, problems);
    }

    private static EcoWittReading ToReading(
        Dictionary<string, double> numbers, string? stationType, string? model, DateTimeOffset? timestamp,
        Dictionary<string, string> unrecognised, List<string> problems)
    {
        double? N(string key) => numbers.TryGetValue(key, out var v) ? v : null;
        return new EcoWittReading
        {
            StationType = stationType,
            Model = model,
            Timestamp = timestamp,
            IndoorTemperatureF = N("tempinf"),
            IndoorHumidity = N("humidityin"),
            OutdoorTemperatureF = N("tempf"),
            OutdoorHumidity = N("humidity"),
            RelativePressureInHg = N("baromrelin"),
            AbsolutePressureInHg = N("baromabsin"),
            WindDirectionDegrees = N("winddir"),
            WindSpeedMph = N("windspeedmph"),
            WindGustMph = N("windgustmph"),
            RainRateInPerHour = N("rainratein"),
            DailyRainIn = N("dailyrainin"),
            WeeklyRainIn = N("weeklyrainin"),
            MonthlyRainIn = N("monthlyrainin"),
            YearlyRainIn = N("yearlyrainin"),
            SolarRadiationWm2 = N("solarradiation"),
            UvIndex = N("uv"),
            Unrecognised = unrecognised,
            Problems = problems,
        };
    }

    private static string Unescape(string text) => Uri.UnescapeDataString(text.Replace('+', ' '));

    private static DateTimeOffset? ParseTimestamp(string value, List<string> problems)
    {
        if (string.Equals(value, "now", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (DateTimeOffset.TryParseExact(value, _timestampFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed))
        {
            return parsed;
        }

        problems.Add($"dateutc='{value}' is not a timestamp");
        return null;
    }
}
