using OoBDev;
using OoBDev.Apache;
using OoBDev.Apache.Tika;
using OoBDev.Apache.Tika.Handlers;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Apache.Tika.Handlers;

/// <summary>
/// Provides functionality to convert Microsoft PowerPoint presentations to HTML using Apache Tika.
/// </summary>
public class TikaPptToHtmlConversionHandler : TikaToHtmlConversionBaseHandler
{
    public static readonly string[] SOURCES = [
        "application/vnd.ms-powerpoint"
        ];

    /// <summary>
    /// Constructor to convert Microsoft PowerPoint presentations to HTML using Apache Tika.
    /// </summary>
    /// <param name="client">client interface</param>
    /// <param name="logger">system logger</param>
    [ExcludeFromCodeCoverage]
    public TikaPptToHtmlConversionHandler(
        IApacheTikaClient client,
        ILogger<TikaPptToHtmlConversionHandler> logger
            ) : base(client, logger)
    {
    }

    /// <summary>
    /// Gets an array of supported source content types for conversion.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public override string[] Sources => SOURCES;
}
