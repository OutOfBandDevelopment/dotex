using OoBDev;
using OoBDev.Apache;
using OoBDev.Apache.Tika;
using OoBDev.Apache.Tika.Handlers;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Apache.Tika.Handlers;

/// <summary>
/// Provides a base class for handlers that convert documents to HTML using Apache Tika.
/// </summary>
public abstract class TikaToHtmlConversionBaseHandler : TikaConversionHandlerBase
{
    public static readonly string[] DESTINATIONS = [
        "text/html",
        "text/xhtml",
        "text/xhtml+xml",
    ];

    /// <summary>
    /// Constructor to convert handler documents to HTML using Apache Tika.
    /// </summary>
    /// <param name="client">client interface</param>
    /// <param name="logger">system logger</param>
    [ExcludeFromCodeCoverage]
    protected TikaToHtmlConversionBaseHandler(
        IApacheTikaClient client,
        ILogger logger
            ) : base(client, logger)
    {
    }

    /// <summary>
    /// Gets an array of supported destination content types for conversion.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public override string[] Destinations => DESTINATIONS;
}
