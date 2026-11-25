using System.Threading;

namespace OoBDev.Documents.Pdf;

/// <summary>Reads the embedded text of a PDF, page by page. Implemented by a PDF library adapter, so the framework itself carries no PDF package.</summary>
public interface IPdfTextReader
{
    /// <summary>Reads the text of every page.</summary>
    /// <param name="path">The PDF; never changed.</param>
    /// <param name="cancellationToken">Cancels the read between pages.</param>
    PdfText Read(string path, CancellationToken cancellationToken);
}
