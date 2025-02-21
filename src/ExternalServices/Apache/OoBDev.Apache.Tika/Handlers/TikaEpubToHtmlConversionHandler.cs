using OoBDev;
using OoBDev.Apache;
using OoBDev.Apache.Tika;
using OoBDev.Apache.Tika.Handlers;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Apache.Tika.Handlers;

/// <summary>
/// Represents a handler for converting EPUB files to HTML using Apache Tika.
/// </summary>
public class TikaEpubToHtmlConversionHandler : TikaToHtmlConversionBaseHandler
{
    public static readonly string[] SOURCES = [
        "application/epub+zip"
        ];

    /// <summary>
    /// Initializes a new instance of the <see cref="TikaEpubToHtmlConversionHandler"/> class.
    /// </summary>
    /// <param name="client">The Apache Tika client used for conversion.</param>
    /// <param name="logger">The logger for logging conversion activities.</param>
    [ExcludeFromCodeCoverage]
    public TikaEpubToHtmlConversionHandler(
        IApacheTikaClient client,
        ILogger<TikaEpubToHtmlConversionHandler> logger
            ) : base(client, logger)
    {
    }

    /// <summary>
    /// Gets an array of supported source content types for conversion.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public override string[] Sources => SOURCES;
}

