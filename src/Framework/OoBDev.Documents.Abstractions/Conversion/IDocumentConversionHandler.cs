using System.IO;
using System.Threading.Tasks;

namespace OoBDev.Documents.Conversion;

/// <summary>
/// Represents a handler for document conversion.
/// </summary>
public interface IDocumentConversionHandler
{
    /// <summary>
    /// Converts a document from one format to another.
    /// </summary>
    /// <param name="source">The source stream containing the document to convert.</param>
    /// <param name="sourceContentType">The content type of the source document.</param>
    /// <param name="destination">The destination stream where the converted document will be written.</param>
    /// <param name="destinationContentType">The desired content type of the converted document.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ConvertAsync(Stream source, string sourceContentType, Stream destination, string destinationContentType);

    /// <summary>
    /// Gets the supported source content types for document conversion.
    /// </summary>
    string[] Sources { get; }

    /// <summary>
    /// Determines whether the specified content type is supported as a source for document conversion.
    /// </summary>
    /// <param name="contentType">The content type to check.</param>
    /// <returns>True if the content type is supported; otherwise, false.</returns>
    bool SupportedSource(string contentType);

    /// <summary>
    /// Gets the supported destination content types for document conversion.
    /// </summary>
    string[] Destinations { get; }

    /// <summary>
    /// Determines whether the specified content type is supported as a destination for document conversion.
    /// </summary>
    /// <param name="contentType">The content type to check.</param>
    /// <returns>True if the content type is supported; otherwise, false.</returns>
    bool SupportedDestination(string contentType);
}
