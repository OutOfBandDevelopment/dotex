namespace OoBDev.Query.Metadata;

/// <summary>How the distance of a <see cref="SpaceAttribute"/> is measured.</summary>
public enum SpaceMetric
{
    /// <summary>Great-circle kilometres between two numeric facets (latitude, longitude) and a position.</summary>
    Haversine,

    /// <summary>Cosine distance between the stored vector of a row and the stored vector of an anchor row.</summary>
    Cosine,
}
