namespace OoBDev.Documents.Models;

/// <summary>
/// Represents a document type, including its name, supported content types, file extensions, and file header.
/// </summary>
public record DocumentType : IDocumentType
{
    /// <summary>
    /// Gets or sets the name of the document type.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the MIME content types supported by this document type.
    /// </summary>
    public required string[] ContentTypes { get; init; }

    /// <summary>
    /// Gets or sets the file extensions commonly associated with this document type.
    /// </summary>
    public required string[] FileExtensions { get; init; }

    /// <summary>
    /// Gets or sets the unique byte sequence at the beginning of files of this type.
    /// </summary>
    public required byte[] FileHeader { get; init; }
}
