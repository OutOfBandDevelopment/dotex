namespace OoBDev.SpatialServices;

/// <summary>The outcome of a lookup; a failed lookup has quality <see cref="ResultQuality.None"/>.</summary>
/// <param name="Position">The position, or null when nothing matched.</param>
/// <param name="Quality">How exact the match is.</param>
/// <param name="NormalizedAddress">The vendor's cleaned-up address.</param>
public sealed record GeocodeResult(GlobalPosition? Position, ResultQuality Quality, string? NormalizedAddress = null)
{
    /// <summary>No match.</summary>
    public static GeocodeResult NoMatch { get; } = new(null, ResultQuality.None);
}
