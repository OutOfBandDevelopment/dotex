namespace OoBDev.Query.Metadata;

/// <summary>The edge expressions of a kind.</summary>
/// <param name="Source">The first end.</param>
/// <param name="Target">The second end.</param>
/// <param name="Weight">The weight.</param>
public sealed record EdgeInfo(string Source, string Target, string Weight);
