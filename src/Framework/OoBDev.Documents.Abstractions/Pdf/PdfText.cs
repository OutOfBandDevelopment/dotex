using System.Collections.Generic;

namespace OoBDev.Documents.Pdf;

/// <summary>The embedded text of a PDF.</summary>
/// <param name="Pages">The trimmed text of each page, in order.</param>
/// <param name="Title">The title from the document information, if any.</param>
public sealed record PdfText(IReadOnlyList<string> Pages, string? Title);
