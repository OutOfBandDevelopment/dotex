using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OoBDev.Documents.Ocr;

namespace OoBDev.Documents.Markdown;

/// <summary>The page-anchored Markdown written from OCR text, and the copy of an OCR result for identical content.</summary>
public static partial class OcrMarkdown
{
    /// <summary>Writes the Markdown for OCR text split at <see cref="IDocumentOcr.PageMark"/>.</summary>
    /// <param name="file">The source PDF.</param>
    /// <param name="markdownPath">The Markdown file to write.</param>
    /// <param name="sha256">The content hash of the source.</param>
    /// <param name="ocrText">The recognised text with page marks.</param>
    /// <param name="extractor">The extractor name, normally <see cref="IDocumentOcr.Extractor"/>.</param>
    public static void Write(string file, string markdownPath, string sha256, string ocrText, string extractor)
    {
        ArgumentNullException.ThrowIfNull(ocrText);
        var pages = ocrText.Split(IDocumentOcr.PageMark).Select(t => t.Trim()).ToList();
        while (pages.Count > 1 && pages[^1].Length == 0)
        {
            pages.RemoveAt(pages.Count - 1);
        }

        var front = MarkdownPages.FrontMatter(Path.GetFileNameWithoutExtension(file), file, sha256, new FileInfo(file).Length, "pdf-ocr", pages.Count, extractor, LanguageDetector.Detect(string.Join(' ', pages)));
        var directory = Path.GetDirectoryName(Path.GetFullPath(markdownPath));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(markdownPath, front + MarkdownPages.Body(pages), new UTF8Encoding(false));
    }

    /// <summary>Copies an OCR Markdown made for the same content under another path, pointing its <c>source:</c> line at <paramref name="file"/>.</summary>
    /// <param name="donorMarkdown">The existing OCR Markdown.</param>
    /// <param name="markdownPath">The copy to write.</param>
    /// <param name="file">The source the copy belongs to.</param>
    /// <returns>False when the donor is missing or is not an OCR result.</returns>
    public static bool CopyFrom(string donorMarkdown, string markdownPath, string file)
    {
        string text;
        try
        {
            if (!File.Exists(donorMarkdown))
            {
                return false;
            }

            text = File.ReadAllText(donorMarkdown, new UTF8Encoding(false, false));
        }
        catch (IOException)
        {
            return false;
        }

        if (!text[..Math.Min(600, text.Length)].Contains("kind: pdf-ocr", StringComparison.Ordinal))
        {
            return false;
        }

        var source = Path.GetFullPath(file).Replace('\\', '/');
        text = SourceLine().Replace(text, _ => $"source: \"{source}\"", 1);
        var directory = Path.GetDirectoryName(Path.GetFullPath(markdownPath));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(markdownPath, text, new UTF8Encoding(false));
        return true;
    }

    [GeneratedRegex("^source: \".*\"$", RegexOptions.Multiline, 1000)]
    private static partial Regex SourceLine();
}
