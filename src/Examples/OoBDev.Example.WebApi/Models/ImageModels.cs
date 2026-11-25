using System.Collections.Generic;

namespace OoBDev.WebApi.Models;

/// <summary>A candidate label and how likely it describes the image.</summary>
public class ImageLabelModel
{
    /// <summary>Gets or sets the label.</summary>
    public required string Label { get; set; }

    /// <summary>Gets or sets the probability, 0 to 1.</summary>
    public float Probability { get; set; }
}

/// <summary>A summary of an image embedding.</summary>
public class ImageEmbeddingModel
{
    /// <summary>Gets or sets the number of values in the vector.</summary>
    public int Length { get; set; }

    /// <summary>Gets or sets the Euclidean length of the vector.</summary>
    public double Norm { get; set; }

    /// <summary>Gets or sets the first values.</summary>
    public required IList<float> First { get; set; }
}
