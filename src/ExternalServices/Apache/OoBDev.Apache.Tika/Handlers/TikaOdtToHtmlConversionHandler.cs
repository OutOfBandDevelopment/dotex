using OoBDev;
using OoBDev.Apache;
using OoBDev.Apache.Tika;
using OoBDev.Apache.Tika.Handlers;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Apache.Tika.Handlers;


/// <summary>
/// Provides functionality to convert OpenDocument Text (ODT) documents to HTML using Apache Tika.
/// </summary>
public class TikaOdtToHtmlConversionHandler : TikaToHtmlConversionBaseHandler
{
    public static readonly string[] SOURCES = [
        "application/vnd.oasis.opendocument.text"
        ];

    /// <summary>
    /// Constructor to convert OpenDocument Text (ODT) documents to HTML using Apache Tika.
    /// </summary>
    /// <param name="client">client interface</param>
    /// <param name="logger">system logger</param>
    [ExcludeFromCodeCoverage]
    public TikaOdtToHtmlConversionHandler(
        IApacheTikaClient client,
        ILogger<TikaOdtToHtmlConversionHandler> logger
            ) : base(client, logger)
    {
    }

    /// <summary>
    /// Gets an array of supported source content types for conversion.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public override string[] Sources => SOURCES;
}
