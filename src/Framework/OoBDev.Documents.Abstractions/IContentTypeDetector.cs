using System.IO;
using System.Threading.Tasks;

namespace OoBDev.Documents;

/// <summary>
/// Represents a contract for detecting content type from a stream.
/// </summary>
public interface IContentTypeDetector
{
    /// <summary>
    /// Asynchronously detects the content type of the provided stream.
    /// </summary>
    /// <param name="source">The stream to detect the content type from.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the detected content type as a string, or <c>null</c> if the content type cannot be determined.</returns>
    Task<string?> DetectContentTypeAsync(Stream source);
}
