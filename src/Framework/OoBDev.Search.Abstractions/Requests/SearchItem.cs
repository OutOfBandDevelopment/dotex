using System.Collections.Generic;

namespace OoBDev.Search.Requests;

/// <summary>
/// One matched item with its score and the reason it matched.
/// </summary>
/// <param name="ItemId">The item key.</param>
/// <param name="Score">The score; higher is better.</param>
/// <param name="Explanation">Why it matched.</param>
/// <param name="MetaData">The item metadata, when the provider has it.</param>
public sealed record SearchItem(string ItemId, double Score, SearchExplanation Explanation, IDictionary<string, object>? MetaData = null);
