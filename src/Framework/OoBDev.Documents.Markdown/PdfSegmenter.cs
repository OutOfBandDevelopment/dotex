using System;
using System.Collections.Generic;
using System.Linq;
using OoBDev.Documents.Pdf;

namespace OoBDev.Documents.Markdown;

/// <summary>
/// Page ranges for OCR in pieces: chapters from the PDF outline are packed together up to <c>maxPages</c>, a longer chapter is cut every <c>maxPages</c>, and a document without an outline
/// is cut every <c>maxPages</c>. A document that fits in one piece is a single range; one that cannot be read is <c>(1, 0)</c> (the caller sends the whole file).
/// </summary>
/// <param name="tools">Inspects the PDF.</param>
public sealed class PdfSegmenter(IPdfPageTools tools)
{
    /// <summary>Plans the ranges for a file.</summary>
    /// <param name="pdfPath">The PDF.</param>
    /// <param name="maxPages">The most pages in one piece; below 1 means no splitting.</param>
    public IReadOnlyList<PageRange> Plan(string pdfPath, int maxPages)
    {
        PdfOutline? outline;
        try
        {
            outline = tools.Inspect(pdfPath);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            outline = null;
        }

        if (outline is null)
        {
            return [new PageRange(1, 0)];
        }

        if (maxPages < 1 || outline.Pages <= maxPages)
        {
            return [new PageRange(1, Math.Max(outline.Pages, 1))];
        }

        var starts = outline.ChapterStarts.Where(p => p >= 1 && p <= outline.Pages).Append(1).Distinct().Order().ToArray();
        return Pack(starts, outline.Pages, maxPages);
    }

    /// <summary>Chapters (each from its start page to the page before the next) packed greedily into ranges of at most <paramref name="maxPages"/> pages; an oversized chapter is cut on its own.</summary>
    /// <param name="chapterStarts">Ascending chapter start pages beginning with 1.</param>
    /// <param name="pages">The page count.</param>
    /// <param name="maxPages">The most pages in one range.</param>
    public static IReadOnlyList<PageRange> Pack(IReadOnlyList<int> chapterStarts, int pages, int maxPages)
    {
        ArgumentNullException.ThrowIfNull(chapterStarts);
        var result = new List<PageRange>();
        var open = 0;
        for (var i = 0; i < chapterStarts.Count; i++)
        {
            var from = chapterStarts[i];
            var to = i + 1 < chapterStarts.Count ? chapterStarts[i + 1] - 1 : pages;
            var length = to - from + 1;
            if (length > maxPages)
            {
                if (open != 0)
                {
                    result.Add(new PageRange(open, from - 1));
                    open = 0;
                }

                for (var p = from; p <= to; p += maxPages)
                {
                    result.Add(new PageRange(p, Math.Min(p + maxPages - 1, to)));
                }
            }
            else if (open != 0 && to - open + 1 > maxPages)
            {
                result.Add(new PageRange(open, from - 1));
                open = from;
            }
            else if (open == 0)
            {
                open = from;
            }
        }

        if (open != 0)
        {
            result.Add(new PageRange(open, pages));
        }

        return result;
    }
}
