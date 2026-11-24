using System;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OoBDev.SpatialServices;

namespace OoBDev.Google.Maps;

/// <summary>Google Geocoding API behind <see cref="ILocationServices"/>: forward and reverse. A missing key, a vendor error or a non-OK status gives no match.</summary>
/// <param name="http">The client; its base address defaults to the Google Maps API host when unset.</param>
/// <param name="options">The settings.</param>
public sealed class GoogleLocationServices(HttpClient http, IOptions<GoogleMapsOptions> options) : ILocationServices
{
    /// <summary>The Google Maps API base address.</summary>
    public static Uri DefaultBaseAddress { get; } = new("https://maps.googleapis.com/");

    /// <inheritdoc />
    public Task<GeocodeResult> GeocodeAsync(AddressQuery address, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(address);
        return QueryAsync("address=" + Uri.EscapeDataString(address.ToOneLine()), cancellationToken);
    }

    /// <inheritdoc />
    public Task<GeocodeResult> ReverseGeocodeAsync(GlobalPosition position, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(position);
        var latlng = string.Create(CultureInfo.InvariantCulture, $"{position.Latitude},{position.Longitude}");
        return QueryAsync("latlng=" + latlng, cancellationToken);
    }

    private async Task<GeocodeResult> QueryAsync(string query, CancellationToken cancellationToken)
    {
        var key = options.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(key))
        {
            return GeocodeResult.NoMatch;
        }

        var relative = $"maps/api/geocode/json?{query}&key={Uri.EscapeDataString(key)}";
        var uri = http.BaseAddress is null ? new Uri(DefaultBaseAddress, relative) : new Uri(relative, UriKind.Relative);
        try
        {
            using var response = await http.GetAsync(uri, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return GeocodeResult.NoMatch;
            }

            var body = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            await using var _ = body.ConfigureAwait(false);
            using var doc = await JsonDocument.ParseAsync(body, cancellationToken: cancellationToken).ConfigureAwait(false);
            return Map(doc.RootElement);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            return GeocodeResult.NoMatch;
        }
    }

    private static GeocodeResult Map(JsonElement root)
    {
        if (!root.TryGetProperty("status", out var status) || status.GetString() != "OK"
            || !root.TryGetProperty("results", out var results) || results.ValueKind != JsonValueKind.Array || results.GetArrayLength() == 0)
        {
            return GeocodeResult.NoMatch;
        }

        var first = results[0];
        if (!first.TryGetProperty("geometry", out var geometry) || !geometry.TryGetProperty("location", out var location)
            || !location.TryGetProperty("lat", out var lat) || !location.TryGetProperty("lng", out var lng))
        {
            return GeocodeResult.NoMatch;
        }

        var (latitude, longitude) = (lat.GetDouble(), lng.GetDouble());
        if (latitude is < -90 or > 90 || longitude is < -180 or > 180)
        {
            return GeocodeResult.NoMatch;
        }

        var type = geometry.TryGetProperty("location_type", out var t) ? t.GetString() : null;
        var quality = type switch
        {
            "ROOFTOP" => ResultQuality.Rooftop,
            "RANGE_INTERPOLATED" => ResultQuality.Interpolated,
            _ => ResultQuality.Approximate,
        };
        return new GeocodeResult(new GlobalPosition(latitude, longitude), quality, first.TryGetProperty("formatted_address", out var a) ? a.GetString() : null);
    }
}
