using OoBDev.Documents.Conversion;
using ReverseMarkdown;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.MysticMind;

/// <summary>
/// Converts HTML documents to Markdown format.
/// </summary>
public class HtmlToMarkdownConversionHandler : IDocumentConversionHandler
{
    private readonly Converter _converter;

    /// <summary>
    /// Constructor for HtmlToMarkdownConversionHandler
    /// </summary>
    /// <param name="_converter"></param>
    public HtmlToMarkdownConversionHandler(
        Converter _converter
        ) => this._converter = _converter;

    /// <summary>
    /// Converts the content of a HTML document in the source stream to Markdown format
    /// and writes it to the destination stream.
    /// </summary>
    /// <param name="source">The source stream containing the HTML document.</param>
    /// <param name="sourceContentType">The content type of the source stream.</param>
    /// <param name="destination">The destination stream where the Markdown content will be written.</param>
    /// <param name="destinationContentType">The content type of the destination stream.</param>
    /// <returns>A task representing the asynchronous conversion operation.</returns>
    /// <exception cref="NotSupportedException">Thrown when either the source or destination content type is not supported.</exception>
    public async Task ConvertAsync(Stream source, string sourceContentType, Stream destination, string destinationContentType)
    {
        if (!SupportedSource(sourceContentType)) throw new NotSupportedException($"Source Content Type \"{sourceContentType}\" is not supported");
        if (!SupportedDestination(destinationContentType)) throw new NotSupportedException($"Source Content Type \"{destinationContentType}\" is not supported");

        using var reader = new StreamReader(source, leaveOpen: true);
        using var writer = new StreamWriter(destination, leaveOpen: true) { AutoFlush = true, };
        var markdown = _converter.Convert(await reader.ReadToEndAsync());
        await writer.WriteAsync(markdown);
    }

    /// <summary>
    /// Gets the content types supported for the destination stream.
    /// </summary>
    public string[] Destinations => ["text/markdown", "text/x-markdown", "text/plain"];

    /// <summary>
    /// Checks if the specified content type is supported for the destination stream.
    /// </summary>
    /// <param name="contentType">The content type to check.</param>
    /// <returns>True if the content type is supported; otherwise, false.</returns>
    public bool SupportedDestination(string contentType) => Destinations.Any(t => string.Equals(t, contentType, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Gets the content types supported for the source stream.
    /// </summary>
    public string[] Sources => ["text/html", "text/xhtml", "text/xhtml+xml"];

    /// <summary>
    /// Checks if the specified content type is supported for the source stream.
    /// </summary>
    /// <param name="contentType">The content type to check.</param>
    /// <returns>True if the content type is supported; otherwise, false.</returns>
    public bool SupportedSource(string contentType) => Sources.Any(t => string.Equals(t, contentType, StringComparison.OrdinalIgnoreCase));
}
