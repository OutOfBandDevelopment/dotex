using System.IO;

namespace OoBDev.Documents.Models;

/// <summary>
/// Represents a content reference containing information about content such as a stream, content type, and file name.
/// </summary>
public record ContentReference
{
    /// <summary>
    /// Gets or initializes the content type of the content.
    /// </summary>
    public required Stream Content { get; init; }

    /// <summary>
    /// Gets or initializes the content type of the content.
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// Gets or initializes the file name associated with the content.
    /// </summary>
    public required string FileName { get; init; }
}
