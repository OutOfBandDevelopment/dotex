namespace OoBDev.SpatialServices;

/// <summary>How exact a geocode result is.</summary>
public enum ResultQuality
{
    /// <summary>No match, a vendor error or a timeout.</summary>
    None,

    /// <summary>A region or street, not a building.</summary>
    Approximate,

    /// <summary>Interpolated along a street segment.</summary>
    Interpolated,

    /// <summary>The building itself.</summary>
    Rooftop,
}
