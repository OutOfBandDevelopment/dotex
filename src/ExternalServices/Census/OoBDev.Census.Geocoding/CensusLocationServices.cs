using System;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.SpatialServices;

namespace OoBDev.Census.Geocoding;

/// <summary>US Census one-line address geocoder (no key, rate limited). The service has no reverse address lookup, so reverse returns no match.</summary>
/// <param name="http">The client; its base address defaults to the Census geocoder when unset.</param>
public sealed class CensusLocationServices(HttpClient http) : ILocationServices
{
    /// <summary>The Census geocoder base address.</summary>
    public static Uri DefaultBaseAddress { get; } = new("https://geocoding.geo.census.gov/");

    /// <inheritdoc />
    public async Task<GeocodeResult> GeocodeAsync(AddressQuery address, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(address);
        var relative = $"geocoder/locations/onelineaddress?benchmark=Public_AR_Current&format=json&address={Uri.EscapeDataString(address.ToOneLine())}";
        var uri = http.BaseAddress is null ? new Uri(DefaultBaseAddress, relative) : new Uri(relative, UriKind.Relative);
        try
        {
            using var response = await http.GetAsync(uri, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return GeocodeResult.NoMatch;
            await using var body = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var doc = await JsonDocument.ParseAsync(body, cancellationToken: cancellationToken).ConfigureAwait(false);
            return Map(doc.RootElement);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            return GeocodeResult.NoMatch;
        }
    }

    /// <inheritdoc />
    public Task<GeocodeResult> ReverseGeocodeAsync(GlobalPosition position, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(position);
        return Task.FromResult(GeocodeResult.NoMatch);
    }

    private static GeocodeResult Map(JsonElement root)
    {
        if (!root.TryGetProperty("result", out var result) || !result.TryGetProperty("addressMatches", out var matches)
            || matches.ValueKind != JsonValueKind.Array || matches.GetArrayLength() == 0) return GeocodeResult.NoMatch;

        var match = matches[0];
        if (!match.TryGetProperty("coordinates", out var c) || !c.TryGetProperty("x", out var x) || !c.TryGetProperty("y", out var y)) return GeocodeResult.NoMatch;
        var lon = x.GetDouble();
        var lat = y.GetDouble();
        if (lat is < -90 or > 90 || lon is < -180 or > 180) return GeocodeResult.NoMatch;

        var normalized = match.TryGetProperty("matchedAddress", out var m) ? m.GetString() : null;
        var quality = match.TryGetProperty("tigerLine", out _) ? ResultQuality.Interpolated : ResultQuality.Approximate;
        return new GeocodeResult(new GlobalPosition(lat, lon), quality, normalized);
    }
}
