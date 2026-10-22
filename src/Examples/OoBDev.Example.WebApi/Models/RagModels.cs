using System.Collections.Generic;

namespace OoBDev.WebApi.Models;

/// <summary>A passage the question is answered from.</summary>
public class RagPassage
{
    /// <summary>Gets or sets a name for the passage (shown in the sources).</summary>
    public required string Id { get; set; }

    /// <summary>Gets or sets the passage text.</summary>
    public required string Text { get; set; }
}

/// <summary>Question plus the passages to search.</summary>
public class RagRequestModel
{
    /// <summary>Gets or sets the question.</summary>
    public required string Question { get; set; }

    /// <summary>Gets or sets the passages to search.</summary>
    public required IList<RagPassage> Passages { get; set; }

    /// <summary>Gets or sets how many of the closest passages are given to the model.</summary>
    public int TopK { get; set; } = 3;
}

/// <summary>A passage selected for the answer.</summary>
public class RagSource
{
    /// <summary>Gets or sets the passage name.</summary>
    public required string Id { get; set; }

    /// <summary>Gets or sets the cosine similarity between the question and the passage.</summary>
    public double Score { get; set; }

    /// <summary>Gets or sets the passage text.</summary>
    public required string Text { get; set; }
}

/// <summary>The answer and the passages it was based on.</summary>
public class RagResponseModel
{
    /// <summary>Gets or sets the model answer.</summary>
    public required string Answer { get; set; }

    /// <summary>Gets or sets the passages given to the model, closest first.</summary>
    public required IList<RagSource> Sources { get; set; }
}
