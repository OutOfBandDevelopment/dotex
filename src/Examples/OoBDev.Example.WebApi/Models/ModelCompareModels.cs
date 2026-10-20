using System.Collections.Generic;

namespace OoBDev.WebApi.Models;

/// <summary>Two texts and the models to compare them with.</summary>
public class ModelCompareRequestModel
{
    /// <summary>Gets or sets the first text.</summary>
    public required string A { get; set; }

    /// <summary>Gets or sets the second text.</summary>
    public required string B { get; set; }

    /// <summary>Gets or sets the model keys.</summary>
    public required List<string> Models { get; set; }
}

/// <summary>The similarity one model gives the two texts.</summary>
public class ModelScoreModel
{
    /// <summary>Gets or sets the model key.</summary>
    public required string Model { get; set; }

    /// <summary>Gets or sets the embedding length.</summary>
    public int Length { get; set; }

    /// <summary>Gets or sets the cosine similarity, -1 to 1.</summary>
    public float Similarity { get; set; }

    /// <summary>Gets or sets the time taken, including a first-use download.</summary>
    public double Milliseconds { get; set; }
}
