using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Documents.Conversion;

/// <summary>
/// Represents a document conversion service.
/// </summary>
public partial class DocumentConversion : IDocumentConversion
{
    private readonly IDocumentConversionChainBuilder _chain;
    private readonly IDocumentTypeTools _tools;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentConversion"/> class with the specified document conversion chain builder.
    /// </summary>
    /// <param name="chain">The document conversion chain builder.</param>
    /// <param name="tools">The document type tools.</param>
    /// <param name="logger">The logger.</param>
    public DocumentConversion(
        IDocumentConversionChainBuilder chain,
        IDocumentTypeTools tools,
        ILogger<DocumentConversion> logger
        )
    {
        _chain = chain;
        _tools = tools;
        _logger = logger;
    }

    private readonly Dictionary<(string source, string destination), ChainStep[]> _cache = [];

    /// <summary>
    /// Converts the content of a source stream to a destination stream asynchronously.
    /// </summary>
    /// <param name="source">The source stream containing the content to be converted.</param>
    /// <param name="sourceContentType">The content type of the source content.</param>
    /// <param name="destination">The destination stream where the converted content will be written.</param>
    /// <param name="destinationContentType">The content type of the converted content.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public virtual Task<bool> ConvertAsync(Stream source, string sourceContentType, Stream destination, string destinationContentType) =>
        InternalConvertAsync(source, sourceContentType, destination, destinationContentType, true);
    internal async Task<bool> InternalConvertAsync(Stream source, string sourceContentType, Stream destination, string destinationContentType, bool recurse)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(sourceContentType, nameof(sourceContentType));
        ArgumentNullException.ThrowIfNull(destination, nameof(destination));
        ArgumentNullException.ThrowIfNull(destinationContentType, nameof(destinationContentType));

        if (string.Equals(sourceContentType, destinationContentType, StringComparison.OrdinalIgnoreCase))
        {
            LogBypass(sourceContentType, destinationContentType);
            await source.CopyToAsync(destination).ConfigureAwait(false);
            return true;
        }

        var steps = GetSteps(sourceContentType, destinationContentType);

        #pragma warning disable CA1873 // guarded by IsEnabled
        if (_logger.IsEnabled(LogLevel.Information))
            LogChain(sourceContentType, destinationContentType, string.Join(';', steps.Select(s => s.Handler)));
        #pragma warning restore CA1873

        if (steps.Length == 0)
        {
            if (recurse)
            {
                var detectedType = await _tools.DetectContentTypeAsync(source).ConfigureAwait(false);
                if (detectedType is not null && !string.Equals(sourceContentType, detectedType, StringComparison.OrdinalIgnoreCase))
                {
                    LogDetected(sourceContentType, detectedType);
                    source.Position = 0;
                    return await InternalConvertAsync(source, detectedType, destination, destinationContentType, false).ConfigureAwait(false);
                }
            }
            else
            {
                LogNotSupported(sourceContentType, destinationContentType);
                throw new NotSupportedException($"Conversion from \"{sourceContentType}\" to \"{destinationContentType}\" is not supported");
            }
        }
        else if (steps.Length == 1)
        {
            await steps[0].Handler.ConvertAsync(source, sourceContentType, destination, destinationContentType).ConfigureAwait(false);
            return true;
        }
        else
        {
            await RunStepsAsync(steps, source, destination).ConfigureAwait(false);
            return true;
        }

        return false;
    }

    private ChainStep[] GetSteps(string sourceContentType, string destinationContentType)
    {
        if (_cache.TryGetValue((sourceContentType, destinationContentType), out var cached))
        {
            LogCachedChain(sourceContentType, destinationContentType);
            return cached;
        }

        LogBuildChain(sourceContentType, destinationContentType);
        var steps = _chain.Steps(sourceContentType, destinationContentType);
        _cache.TryAdd((sourceContentType, destinationContentType), steps);
        return steps;
    }

    private static async Task RunStepsAsync(ChainStep[] steps, Stream source, Stream destination)
    {
        MemoryStream? temp = null;
        foreach (var step in steps)
        {
            temp = new MemoryStream();
            await step.Handler.ConvertAsync(source, step.SourceContentType, temp, step.DestinationContentType).ConfigureAwait(false);
            temp.Position = 0;
            source = temp;
        }
        await (temp ?? source).CopyToAsync(destination).ConfigureAwait(false);
        destination.Position = 0;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Bypass as input and output types match {source} -> {destination}")]
    private partial void LogBypass(string source, string destination);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Use cached chain {source} -> {destination}")]
    private partial void LogCachedChain(string source, string destination);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Build chain {source} -> {destination}")]
    private partial void LogBuildChain(string source, string destination);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Chain {source} -> {destination} [{steps}]")]
    private partial void LogChain(string source, string destination, string steps);

    [LoggerMessage(EventId = 5, Level = LogLevel.Warning, Message = "Provided \"{source}\" but detected \"{detected}\" to try again")]
    private partial void LogDetected(string source, string detected);

    [LoggerMessage(EventId = 6, Level = LogLevel.Error, Message = "Conversion from \"{sourceContentType}\" to \"{destinationContentType}\" is not supported")]
    private partial void LogNotSupported(string sourceContentType, string destinationContentType);
}

