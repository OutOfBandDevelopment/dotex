using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using OoBDev.Documents.Conversion;
using HtmlToOpenXml;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.HtmlToOpenXml;

/// <summary>
/// Handler for converting HTML content to Docx format.
/// </summary>
public class HtmlToDocxConversionHandler : IDocumentConversionHandler
{
    /// <summary>
    /// Target Content Types
    /// </summary>
    public static readonly string[] DESTINATIONS = [
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    ];

    /// <summary>
    /// Converts HTML content from a source stream to Docx format and writes it to a destination stream.
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

        source.Position = 0;
        using var htmlReader = new StreamReader(source);
        var html = htmlReader.ReadToEnd();

        using (var generatedDocument = new MemoryStream())
        {
            using (var package = WordprocessingDocument.Create(generatedDocument, WordprocessingDocumentType.Document))
            {
                var mainPart = package.MainDocumentPart;
                if (mainPart == null)
                {
                    mainPart = package.AddMainDocumentPart();
                    new Document(new Body()).Save(mainPart);
                }
                var converter = new HtmlConverter(mainPart);
                await converter.ParseBody(html);
                mainPart.Document.Save();
            }

            await destination.WriteAsync(generatedDocument.ToArray());
            destination.Position = 0;
        }
    }

    /// <summary>
    /// Gets the supported content types for destination PDF files.
    /// </summary>
    public string[] Destinations => DESTINATIONS;

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
