using System.Collections.Generic;

namespace OoBDev.WebApi.Models;

/// <summary>The hits of the three search modes for one query.</summary>
public class SearchResponseModel
{
    /// <summary>Gets or sets the keyword (BM25) hits, best first.</summary>
    public required IList<SearchHitModel> Lexical { get; set; }

    /// <summary>Gets or sets the embedding similarity hits, best first.</summary>
    public required IList<SearchHitModel> Semantic { get; set; }

    /// <summary>Gets or sets the reciprocal rank fusion of both lists, best first.</summary>
    public required IList<SearchHitModel> Hybrid { get; set; }

    /// <summary>Gets or sets how many passages were left out because the caller lacks the right they require.</summary>
    public int Hidden { get; set; }
}
