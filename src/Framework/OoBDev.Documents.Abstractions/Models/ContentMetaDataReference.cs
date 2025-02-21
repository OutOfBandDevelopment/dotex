using System.Collections.Generic;

namespace OoBDev.Documents.Models;

/// <summary>
/// Represents a reference to content metadata.
/// </summary>
public record ContentMetaDataReference
{

    /// <summary>
    /// Gets or initializes the content type of the content.
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// Gets or initializes the file name associated with the content.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Gets or initializes the metadata associated with the content.
    /// </summary>
    public Dictionary<string, string>? MetaData { get; init; }
}
