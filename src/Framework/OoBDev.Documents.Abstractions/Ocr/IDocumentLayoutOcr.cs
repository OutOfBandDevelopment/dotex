using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Documents.Ocr;

/// <summary>An <see cref="IDocumentOcr"/> that also reports where each block of text sits on its page. Failure rules are those of <see cref="IDocumentOcr"/>.</summary>
public interface IDocumentLayoutOcr : IDocumentOcr
{
    /// <summary>Recognises one PDF file and returns its Markdown with the text regions.</summary>
    /// <param name="pdfPath">The PDF to read; it is never changed.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The Markdown and the regions.</returns>
    Task<OcrLayout> RecognizeLayoutAsync(string pdfPath, CancellationToken cancellationToken);
}
