using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Documents.Ocr;

/// <summary>
/// Reads a scanned PDF and returns Markdown with <see cref="PageMark"/> between pages. Throws
/// <see cref="System.Net.Http.HttpRequestException"/> or <see cref="System.TimeoutException"/> when the service is down or too slow
/// (the caller retries) and <see cref="OcrFailedException"/> when the service rejects the document (final).
/// </summary>
public interface IDocumentOcr
{
    /// <summary>The text placed between pages in the returned Markdown.</summary>
    public const string PageMark = "<<<PAGE>>>";

    /// <summary>The extractor name written to the Markdown front matter.</summary>
    string Extractor { get; }

    /// <summary>Recognises the text of one PDF file.</summary>
    /// <param name="pdfPath">The PDF to read; it is never changed.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>Markdown with <see cref="PageMark"/> between pages.</returns>
    Task<string> RecognizeAsync(string pdfPath, CancellationToken cancellationToken);
}
