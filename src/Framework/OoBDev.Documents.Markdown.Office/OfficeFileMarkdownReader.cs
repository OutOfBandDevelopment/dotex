using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using OoBDev.Documents.Markdown;

namespace OoBDev.Documents.Markdown.Office;

/// <summary>
/// Reads a word-processing or e-book file as Markdown through the registered conversion chain (an adapter converts the file to
/// HTML, another turns HTML into Markdown), so the framework carries no office library and no service address.
/// </summary>
/// <param name="conversion">The document conversion service with its handlers registered.</param>
public sealed class OfficeFileMarkdownReader(IDocumentConversion conversion) : IFileMarkdownReader
{
    private const string MarkdownType = "text/markdown";

    private static readonly Dictionary<string, string> _types = new(StringComparer.OrdinalIgnoreCase)
    {
        [".doc"] = "application/msword",
        [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        [".odt"] = "application/vnd.oasis.opendocument.text",
        [".rtf"] = "application/rtf",
        [".epub"] = "application/epub+zip",
    };

    /// <inheritdoc />
    public IReadOnlyCollection<string> Extensions { get; } = [.. _types.Keys];

    /// <inheritdoc />
    public string Extractor => "office via conversion chain";

    /// <inheritdoc />
    /// <exception cref="NotSupportedException">The extension is not an office type, or no registered chain reaches Markdown.</exception>
    public string Read(string path, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        if (!_types.TryGetValue(Path.GetExtension(path), out var contentType))
        {
            throw new NotSupportedException($"No office content type is known for \"{Path.GetExtension(path)}\".");
        }

        // The reader contract is synchronous; the conversion is awaited on the caller's thread.
        return ReadAsync(path, contentType, cancellationToken).GetAwaiter().GetResult();
    }

    private async System.Threading.Tasks.Task<string> ReadAsync(string path, string contentType, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await using var source = File.OpenRead(path);
        using var destination = new MemoryStream();
        if (!await conversion.ConvertAsync(source, contentType, destination, MarkdownType).ConfigureAwait(false))
        {
            return string.Empty;
        }

        return Encoding.UTF8.GetString(destination.ToArray());
    }
}
