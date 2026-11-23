namespace OoBDev.Google.Maps;

/// <summary>Settings for the Google Geocoding API.</summary>
public sealed class GoogleMapsOptions
{
    /// <summary>The configuration section.</summary>
    public const string SectionName = "OoBDev:Google:Maps";

    /// <summary>The API key; without one every lookup returns no match.</summary>
    public string? ApiKey { get; set; }
}
