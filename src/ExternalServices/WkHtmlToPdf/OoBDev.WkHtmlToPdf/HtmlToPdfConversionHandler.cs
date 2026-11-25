using Microsoft.Extensions.Options;
using OoBDev.Documents.Conversion;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

namespace OoBDev.WkHtmlToPdf;

/// <summary>
/// Handler for converting HTML content to PDF format.
/// </summary>
public class HtmlToPdfConversionHandler : IDocumentConversionHandler
{
    private readonly IConverter _converter;
    private readonly HtmlToPdfOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlToPdfConversionHandler"/> class.
    /// </summary>
    /// <param name="converter">The converter used for HTML to PDF conversion.</param>
    /// <param name="options">The page settings.</param>
    public HtmlToPdfConversionHandler(IConverter converter, IOptions<HtmlToPdfOptions> options)
    {
        _converter = converter;
        _options = options.Value;
    }

    /// <summary>
    /// Converts HTML content from a source stream to PDF format and writes it to a destination stream.
    /// </summary>
    /// <param name="source">The source stream containing the HTML content.</param>
    /// <param name="sourceContentType">The content type of the source.</param>
    /// <param name="destination">The destination stream to write the PDF content.</param>
    /// <param name="destinationContentType">The content type of the destination.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="NotSupportedException">Thrown if the source or destination content type is not supported.</exception>
    public async Task ConvertAsync(Stream source, string sourceContentType, Stream destination, string destinationContentType)
    {
        if (!SupportedSource(sourceContentType)) throw new NotSupportedException($"Source Content Type \"{sourceContentType}\" is not supported");
        if (!SupportedDestination(destinationContentType)) throw new NotSupportedException($"Destination Content Type \"{destinationContentType}\" is not supported");

        using var reader = new StreamReader(source, leaveOpen: true);
        var html = await reader.ReadToEndAsync().ConfigureAwait(false);

        var pdf = _converter.Convert(CreateDocument(html));
        await destination.WriteAsync(pdf).ConfigureAwait(false);
    }

    /// <summary>
    /// Builds the wkhtmltopdf document for the HTML using the configured page settings.
    /// </summary>
    /// <param name="html">The HTML to render.</param>
    /// <returns>The document handed to the converter.</returns>
    public HtmlToPdfDocument CreateDocument(string html) => new()
    {
        GlobalSettings = {
            ColorMode = _options.ColorMode,
            Orientation = _options.Orientation,
            PaperSize = _options.PaperSize,
        },
        Objects = {
            new () {
                PagesCount = true,
                HtmlContent = html,
                WebSettings = { DefaultEncoding = _options.DefaultEncoding },
                HeaderSettings = { FontSize = _options.HeaderFontSize, Right = _options.HeaderRight, Line = _options.HeaderLine, Spacing = 2.812 }
            }
        }
    };

    /// <summary>
    /// Gets the supported content types for destination PDF files.
    /// </summary>
    public string[] Destinations => ["application/pdf"];

    /// <summary>
    /// Checks if the specified content type is supported for PDF conversion.
    /// </summary>
    /// <param name="contentType">The content type to check.</param>
    /// <returns><c>true</c> if the content type is supported; otherwise, <c>false</c>.</returns>
    public bool SupportedDestination(string contentType) => Destinations.Any(t => string.Equals(t, contentType, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Gets the supported content types for source HTML files.
    /// </summary>
    public string[] Sources => ["text/html", "text/xhtml", "text/xhtml+xml"];

    /// <summary>
    /// Checks if the specified content type is supported as source HTML for PDF conversion.
    /// </summary>
    /// <param name="contentType">The content type to check.</param>
    /// <returns><c>true</c> if the content type is supported; otherwise, <c>false</c>.</returns>
    public bool SupportedSource(string contentType) => Sources.Any(t => string.Equals(t, contentType, StringComparison.OrdinalIgnoreCase));

}
