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

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlToPdfConversionHandler"/> class.
    /// </summary>
    /// <param name="converter">The converter used for HTML to PDF conversion.</param>
    public HtmlToPdfConversionHandler(IConverter converter) => _converter = converter;

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
        if (!SupportedDestination(destinationContentType)) throw new NotSupportedException($"Source Content Type \"{destinationContentType}\" is not supported");

        using var reader = new StreamReader(source, leaveOpen: true);
        var html = await reader.ReadToEndAsync();

        //TODO: change this to config stuff or create a conversion context/extension so this can be provided per instance
        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.Letter,
            },
            Objects = {
                new () {
                    PagesCount = true,
                    HtmlContent = html,
                    WebSettings = { DefaultEncoding = "utf-8" },
                    HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true, Spacing = 2.812 }
                }
            }
        };

        var pdf = _converter.Convert(doc);
        await destination.WriteAsync(pdf);
    }

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
