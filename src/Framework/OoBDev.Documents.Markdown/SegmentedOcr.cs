using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.Documents.Ocr;
using OoBDev.Documents.Pdf;

namespace OoBDev.Documents.Markdown;

/// <summary>
/// OCR of a long document in pieces that resume after a retry: each finished piece's text is kept under the work folder, so a service that is down or too slow costs only the piece in
/// flight. The folder is removed once the whole text is assembled. Page numbers stay true to the source.
/// </summary>
/// <param name="ocr">The OCR service.</param>
/// <param name="tools">Writes page ranges.</param>
public sealed class SegmentedOcr(IDocumentOcr ocr, IPdfPageTools tools)
{
    private readonly PdfSegmenter segmenter = new(tools);

    /// <summary>Recognises <paramref name="file"/>.</summary>
    /// <param name="file">The PDF.</param>
    /// <param name="workDirectory">Where finished pieces are kept; created if needed.</param>
    /// <param name="maxPages">The most pages in one piece.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>Markdown with <see cref="IDocumentOcr.PageMark"/> between pages.</returns>
    public async Task<string> RecognizeAsync(string file, string workDirectory, int maxPages, CancellationToken cancellationToken)
    {
        var plan = segmenter.Plan(file, maxPages);
        if (plan.Count <= 1)
        {
            return await ocr.RecognizeAsync(file, cancellationToken).ConfigureAwait(false);
        }

        Directory.CreateDirectory(workDirectory);
        var pages = new List<string>();
        for (var i = 0; i < plan.Count; i++)
        {
            var (from, to) = (plan[i].From, plan[i].To);
            var textPath = Path.Combine(workDirectory, $"seg-{i:000}.txt");
            string recognized;
            if (File.Exists(textPath))
            {
                recognized = await File.ReadAllTextAsync(textPath, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var piece = Path.Combine(workDirectory, $"seg-{i:000}.pdf");
                tools.WritePages(file, from, to, piece);
                recognized = await ocr.RecognizeAsync(piece, cancellationToken).ConfigureAwait(false);
                await File.WriteAllTextAsync(textPath, recognized, cancellationToken).ConfigureAwait(false);
                File.Delete(piece);
            }

            var own = recognized.Split(IDocumentOcr.PageMark).ToList();
            while (own.Count > 1 && own[^1].Trim().Length == 0)
            {
                own.RemoveAt(own.Count - 1);
            }

            var count = to - from + 1;
            if (own.Count > count)
            {
                own = [.. own.Take(count - 1), string.Join("\n\n", own.Skip(count - 1))];
            }

            while (own.Count < count)
            {
                own.Add("");
            }

            pages.AddRange(own);
        }

        try
        {
            Directory.Delete(workDirectory, true);
        }
        catch (IOException)
        {
            // a leftover folder is only disk
        }

        return string.Join(IDocumentOcr.PageMark, pages);
    }
}
