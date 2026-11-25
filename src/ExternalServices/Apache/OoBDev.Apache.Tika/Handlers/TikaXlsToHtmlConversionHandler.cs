using OoBDev;
using OoBDev.Apache;
using OoBDev.Apache.Tika;
using OoBDev.Apache.Tika.Handlers;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Apache.Tika.Handlers;

/// <summary>
/// Provides functionality to convert Microsoft Excel workbooks to HTML using Apache Tika.
/// </summary>
public class TikaXlsToHtmlConversionHandler : TikaToHtmlConversionBaseHandler
{
    public static readonly string[] SOURCES = [
        "application/vnd.ms-excel"
        ];

    /// <summary>
    /// Constructor to convert Microsoft Excel workbooks to HTML using Apache Tika.
    /// </summary>
    /// <param name="client">client interface</param>
    /// <param name="logger">system logger</param>
    [ExcludeFromCodeCoverage]
    public TikaXlsToHtmlConversionHandler(
        IApacheTikaClient client,
        ILogger<TikaXlsToHtmlConversionHandler> logger
            ) : base(client, logger)
    {
    }

    /// <summary>
    /// Gets an array of supported source content types for conversion.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public override string[] Sources => SOURCES;
}
