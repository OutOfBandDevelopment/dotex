using System.Collections.Generic;

namespace OoBDev.Documents.Pdf;

/// <summary>The structure of a PDF as far as OCR planning needs it.</summary>
/// <param name="Pages">Number of pages.</param>
/// <param name="ChapterStarts">Start pages of the top-level bookmarks, ascending; may be empty.</param>
public sealed record PdfOutline(int Pages, IReadOnlyList<int> ChapterStarts);
