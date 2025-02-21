namespace OoBDev.Documents.Models;

/// <summary>
/// Represents a document type.
/// </summary>
public interface IDocumentType
{
    /// <summary>
    /// Gets the name of the document type.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the supported content types for documents of this type.
    /// </summary>
    string[] ContentTypes { get; }

    /// <summary>
    /// Gets the supported file extensions for documents of this type.
    /// </summary>
    string[] FileExtensions { get; }

    /// <summary>
    /// Gets the file header bytes that identify documents of this type.
    /// </summary>
    byte[] FileHeader { get; }
}
