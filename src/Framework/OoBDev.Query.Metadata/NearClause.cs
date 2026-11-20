namespace OoBDev.Query.Metadata;

/// <summary>Rows ordered by closeness to an anchor in a space the kind declares.</summary>
/// <param name="Space">The space name.</param>
/// <param name="Ref">Vector spaces: the key of the anchor row.</param>
/// <param name="Lat">Geo spaces: the anchor latitude.</param>
/// <param name="Lon">Geo spaces: the anchor longitude.</param>
/// <param name="Max">The farthest distance to return, in the space's unit.</param>
/// <param name="Text">Vector spaces: words to find similar rows for. The engine embeds them with the configured <c>IEmbeddingProvider</c>; without one the request answers 400.</param>
/// <param name="Vector">Vector spaces: the embedded anchor, set by the engine from <paramref name="Text"/>; callers may also supply it.</param>
public sealed record NearClause(string Space, string? Ref = null, double? Lat = null, double? Lon = null, double? Max = null, string? Text = null, float[]? Vector = null);
