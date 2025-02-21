
using OoBDev.Documents.Models;
using System.IO;
using System.Threading.Tasks;

namespace OoBDev.Documents;

/// <summary>
/// Provides tools for working with document types.
/// </summary>
public interface IDocumentTypeTools
{
    /// <summary>
    /// Scan content to detect content type
    /// </summary>
    /// <param name="source">stream</param>
    /// <returns>content type</returns>
    Task<string?> DetectContentTypeAsync(Stream source);

    /// <summary>
    /// Retrieves the document type associated with the specified content type.
    /// </summary>
    /// <param name="contentType">The content type to search for.</param>
    /// <returns>The document type associated with the specified content type, or null if no matching document type is found.</returns>
    IDocumentType? GetByContentType(string contentType);

    /// <summary>
    /// Retrieves the document type associated with the specified file extension.
    /// </summary>
    /// <param name="fileExtension">The file extension to search for.</param>
    /// <returns>The document type associated with the specified file extension, or null if no matching document type is found.</returns>
    IDocumentType? GetByFileExtension(string fileExtension);

    /// <summary>
    /// Retrieves the document type associated with the content identified by the file header in the specified stream.
    /// </summary>
    /// <param name="stream">The stream containing the file header.</param>
    /// <returns>The document type associated with the content identified by the file header, or null if no matching document type is found.</returns>
    IDocumentType? GetByFileHeader(Stream stream);
}
