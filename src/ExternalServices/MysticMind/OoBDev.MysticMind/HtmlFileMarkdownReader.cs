using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using OoBDev.Documents.Markdown;
using ReverseMarkdown;

namespace OoBDev.MysticMind;

/// <summary>Reads an HTML or XHTML file as Markdown with ReverseMarkdown, for <c>DocumentMarkdownConverter</c>.</summary>
/// <param name="converter">The configured ReverseMarkdown converter.</param>
public sealed class HtmlFileMarkdownReader(Converter converter) : IFileMarkdownReader
{
    /// <inheritdoc />
    public IReadOnlyCollection<string> Extensions { get; } = [".html", ".htm", ".xhtml"];

    /// <inheritdoc />
    public string Extractor => "reversemarkdown html";

    /// <inheritdoc />
    public string Read(string path, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        return converter.Convert(File.ReadAllText(path));
    }
}
