using OoBDev;
using OoBDev.Apache;
using OoBDev.Apache.Tika;
using OoBDev.Apache.Tika.Handlers;
using OoBDev.Documents.Conversion;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Apache.Tika.Handlers;

/// <summary>
/// Provides a base class for document conversion handlers using Apache Tika.
/// </summary>
public abstract class TikaConversionHandlerBase : IDocumentConversionHandler
{
    private readonly IApacheTikaClient _client;
    private readonly ILogger _logger;

    protected TikaConversionHandlerBase(
        IApacheTikaClient client,
        ILogger logger
            )
    {
        _client = client;
        _logger = logger;
    }

    /// <summary>
    /// Gets an array of supported destination content types for conversion.
    /// </summary>
    public abstract string[] Destinations { get; }

    /// <summary>
    /// Gets an array of supported source content types for conversion.
    /// </summary>
    public abstract string[] Sources { get; }

    /// <summary>
    /// Asynchronously converts a document from the source stream to the destination stream.
    /// </summary>
    /// <param name="source">The stream containing the source document.</param>
    /// <param name="sourceContentType">The content type of the source document.</param>
    /// <param name="destination">The stream to which the converted document will be written.</param>
    /// <param name="destinationContentType">The desired content type of the converted document.</param>
    /// <exception cref="NotSupportedException">
    /// Thrown when either the source content type or the destination content type is not supported.
    /// </exception>
    public async Task ConvertAsync(Stream source, string sourceContentType, Stream destination, string destinationContentType)
    {
        if (!SupportedSource(sourceContentType)) throw new NotSupportedException($"Source Content Type \"{sourceContentType}\" is not supported");
        if (!SupportedDestination(destinationContentType)) throw new NotSupportedException($"Source Content Type \"{destinationContentType}\" is not supported");

        await _client.ConvertAsync(source, sourceContentType, destination, destinationContentType);
    }

    /// <summary>
    /// Checks if the specified content type is supported for conversion to a destination format.
    /// </summary>
    /// <param name="contentType">The content type to check.</param>
    /// <returns><c>true</c> if the content type is supported; otherwise, <c>false</c>.</returns>
    public bool SupportedDestination(string contentType) => Destinations.Any(t => string.Equals(t, contentType, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Checks if the specified content type is supported as a source format for conversion.
    /// </summary>
    /// <param name="contentType">The content type to check.</param>
    /// <returns><c>true</c> if the content type is supported; otherwise, <c>false</c>.</returns>
    public bool SupportedSource(string contentType) => Sources.Any(t => string.Equals(t, contentType, StringComparison.OrdinalIgnoreCase));

}
