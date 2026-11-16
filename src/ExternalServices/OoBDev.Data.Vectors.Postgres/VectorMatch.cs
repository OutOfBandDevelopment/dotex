using OoBDev.Search.Models;

namespace OoBDev.Data.Vectors.Postgres;

/// <summary>
/// A neighbour with the explanation of the match: which of the item's vectors matched and how far it is.
/// </summary>
/// <param name="Result">The search result; its score is derived from <paramref name="Distance"/>.</param>
/// <param name="VectorIndex">The position of the matching vector within the item (for example the page).</param>
/// <param name="Distance">The raw pgvector distance.</param>
public sealed record VectorMatch(SearchResultModel Result, int VectorIndex, double Distance);
