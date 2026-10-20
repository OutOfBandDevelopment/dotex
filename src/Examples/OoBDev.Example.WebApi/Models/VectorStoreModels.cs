using System.Collections.Generic;

namespace OoBDev.WebApi.Models;

/// <summary>Documents to embed and store in a collection.</summary>
public class VectorIndexRequestModel
{
    /// <summary>Gets or sets the collection name.</summary>
    public required string Collection { get; set; }

    /// <summary>Gets or sets the texts to embed; each is stored with its text as metadata.</summary>
    public required List<string> Texts { get; set; }
}

/// <summary>Ids of the stored vectors.</summary>
public class VectorIndexResponseModel
{
    /// <summary>Gets or sets the stored ids.</summary>
    public required string[] Ids { get; set; }

    /// <summary>Gets or sets the backing store: sql or memory.</summary>
    public required string Store { get; set; }
}

/// <summary>A similarity query.</summary>
public class VectorQueryRequestModel
{
    /// <summary>Gets or sets the collection name.</summary>
    public required string Collection { get; set; }

    /// <summary>Gets or sets the query text.</summary>
    public required string Query { get; set; }

    /// <summary>Gets or sets the maximum hits.</summary>
    public int Limit { get; set; } = 5;
}

/// <summary>One stored vector and its distance from the query.</summary>
public class VectorHitModel
{
    /// <summary>Gets or sets the vector id.</summary>
    public required string Id { get; set; }

    /// <summary>Gets or sets the distance (lower is closer; 0 when listing).</summary>
    public double Score { get; set; }

    /// <summary>Gets or sets the stored text.</summary>
    public string? Text { get; set; }
}
