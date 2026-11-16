using System;

namespace OoBDev.Search.Requests;

/// <summary>
/// A reference the results are ordered by closeness to: an existing item (<c>like</c>), a vector or a text (<c>near</c>).
/// </summary>
/// <param name="LikeItemId">An item whose vector is the query.</param>
/// <param name="NearVector">A vector to search near.</param>
/// <param name="NearText">A text to embed and search near.</param>
public sealed record SearchAnchor(string? LikeItemId = null, ReadOnlyMemory<float>? NearVector = null, string? NearText = null);
