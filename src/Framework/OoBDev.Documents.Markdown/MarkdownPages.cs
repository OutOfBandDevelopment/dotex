using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace OoBDev.Documents.Markdown;

/// <summary>Builds the page-anchored Markdown body and the front matter every converted document starts with.</summary>
public static class MarkdownPages
{
    /// <summary>One <c>&lt;a id="pN"&gt;</c> anchor and "## Page N" heading per page.</summary>
    /// <param name="pages">The page texts, in order.</param>
    public static string Body(IEnumerable<string> pages)
    {
        var sb = new StringBuilder();
        var n = 0;
        foreach (var text in pages)
        {
            n++;
            var number = n.ToString(CultureInfo.InvariantCulture);
            sb.Append("<a id=\"p").Append(number).Append("\"></a>\n## Page ").Append(number).Append("\n\n").Append(text).Append("\n\n");
        }

        return sb.ToString();
    }

    /// <summary>The YAML front matter block, ending with a blank line.</summary>
    /// <param name="title">The title; double quotes become single quotes.</param>
    /// <param name="sourcePath">The source file; made absolute with forward slashes.</param>
    /// <param name="sha256">The content hash of the source.</param>
    /// <param name="size">The source size in bytes.</param>
    /// <param name="kind">The source kind such as pdf, pdf-ocr or text.</param>
    /// <param name="pages">The page count.</param>
    /// <param name="extractor">The extractor name.</param>
    public static string FrontMatter(string title, string sourcePath, string sha256, long size, string kind, int pages, string extractor)
    {
        var source = Path.GetFullPath(sourcePath).Replace('\\', '/');
        return string.Create(CultureInfo.InvariantCulture,
            $"---\ntitle: \"{title.Replace('"', '\'')}\"\nsource: \"{source}\"\nsha256: {sha256}\nsize: {size}\nkind: {kind}\npages: {pages}\nextractor: {extractor}\n---\n\n");
    }
}
