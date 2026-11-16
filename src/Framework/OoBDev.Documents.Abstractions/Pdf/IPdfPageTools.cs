namespace OoBDev.Documents.Pdf;

/// <summary>Inspects a PDF and writes a page range as a PDF of its own, for OCR in pieces.</summary>
public interface IPdfPageTools
{
    /// <summary>Reads the page count and the top-level chapter start pages; null when the file cannot be read (the caller then uses the whole file).</summary>
    /// <param name="path">The PDF; never changed.</param>
    PdfOutline? Inspect(string path);

    /// <summary>Writes pages <paramref name="from"/> to <paramref name="to"/> (1-based, inclusive) as a PDF at <paramref name="outputPath"/>.</summary>
    /// <param name="path">The source PDF; never changed.</param>
    /// <param name="from">First page.</param>
    /// <param name="to">Last page.</param>
    /// <param name="outputPath">The file to write.</param>
    void WritePages(string path, int from, int to, string outputPath);
}
