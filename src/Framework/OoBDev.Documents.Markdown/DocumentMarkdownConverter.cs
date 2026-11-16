using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using OoBDev.Documents.Pdf;

namespace OoBDev.Documents.Markdown;

/// <summary>
/// Plain text and PDF to page-anchored Markdown with front matter. A PDF whose pages average under <see cref="MinCharsPerPage"/> characters needs OCR instead.
/// </summary>
/// <param name="pdfText">Reads the embedded text of a PDF.</param>
/// <param name="readers">Format readers (HTML, office) tried by extension before the plain-text fallback.</param>
public sealed class DocumentMarkdownConverter(IPdfTextReader pdfText, IEnumerable<IFileMarkdownReader>? readers = null)
{
    /// <summary>The average characters per page below which a PDF counts as a scan.</summary>
    public const int MinCharsPerPage = 40;

    /// <summary>The extractor name for embedded PDF text.</summary>
    public const string PdfExtractor = "pdf text";

    /// <summary>Converts <paramref name="file"/> and writes the Markdown to <paramref name="markdownPath"/>.</summary>
    /// <param name="file">The source (.pdf, otherwise text).</param>
    /// <param name="markdownPath">The Markdown file; an existing file is left alone.</param>
    /// <param name="sha256">The content hash of the source, written to the front matter.</param>
    /// <param name="cancellationToken">Cancels between pages.</param>
    public ConversionResult Convert(string file, string markdownPath, string sha256, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(file);
        ArgumentException.ThrowIfNullOrEmpty(markdownPath);
        if (File.Exists(markdownPath))
        {
            return new ConversionResult(ConversionStatus.Unchanged, markdownPath.Replace('\\', '/'));
        }

        string body, title, kind, extractor;
        int pages;
        if (string.Equals(Path.GetExtension(file), ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            if (!HasPdfHeader(file))
            {
                return new ConversionResult(ConversionStatus.NotPdf, "no %PDF header");
            }

            var text = pdfText.Read(file, cancellationToken);
            pages = text.Pages.Count;
            var chars = text.Pages.Sum(t => t.EnumerateRunes().Count());
            if (pages == 0 || (double)chars / pages < MinCharsPerPage)
            {
                return new ConversionResult(ConversionStatus.NeedsOcr, pages + " pages");
            }

            body = MarkdownPages.Body(text.Pages);
            title = string.IsNullOrWhiteSpace(text.Title) ? Path.GetFileNameWithoutExtension(file) : text.Title.Trim();
            kind = "pdf";
            extractor = PdfExtractor;
        }
        else if (FindReader(file) is { } reader)
        {
            body = reader.Read(file, cancellationToken).Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
            if (body.Trim().Length == 0)
            {
                return new ConversionResult(ConversionStatus.Empty, "no text found");
            }

            title = Path.GetFileNameWithoutExtension(file);
            kind = Path.GetExtension(file).TrimStart('.').ToLowerInvariant();
            extractor = reader.Extractor;
            pages = 1;
        }
        else
        {
            body = new UTF8Encoding(false, false).GetString(File.ReadAllBytes(file)).Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
            if (body.Trim().Length == 0)
            {
                return new ConversionResult(ConversionStatus.Empty, "no text found");
            }

            title = Path.GetFileNameWithoutExtension(file);
            kind = "text";
            extractor = "text";
            pages = 1;
        }

        var directory = Path.GetDirectoryName(Path.GetFullPath(markdownPath));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var front = MarkdownPages.FrontMatter(title, file, sha256, new FileInfo(file).Length, kind, pages, extractor);
        File.WriteAllText(markdownPath, front + body, new UTF8Encoding(false));
        return new ConversionResult(ConversionStatus.Converted, markdownPath.Replace('\\', '/'));
    }

    private IFileMarkdownReader? FindReader(string file)
    {
        var extension = Path.GetExtension(file);
        return readers?.FirstOrDefault(r => r.Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase));
    }

    private static bool HasPdfHeader(string file)
    {
        using var stream = File.OpenRead(file);
        var head = new byte[Math.Min(1024, stream.Length)];
        stream.ReadExactly(head);
        return Encoding.Latin1.GetString(head).Contains("%PDF", StringComparison.Ordinal);
    }
}
