using System.IO;
using System.Threading.Tasks;

namespace OoBDev.Documents;

/// <summary>
/// Represents a service for document conversion.
/// </summary>
public interface IDocumentConversion
{
    /// <summary>
    /// Converts a document from one format to another.
    /// </summary>
    /// <param name="source">The source stream containing the document to convert.</param>
    /// <param name="sourceContentType">The content type of the source document.</param>
    /// <param name="destination">The destination stream where the converted document will be written.</param>
    /// <param name="destinationContentType">The desired content type of the converted document.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<bool> ConvertAsync(Stream source, string sourceContentType, Stream destination, string destinationContentType);
}
