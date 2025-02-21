using OoBDev;
using OoBDev.Apache;
using OoBDev.Apache.Tika;
using OoBDev.Apache.Tika.Detectors;
using OoBDev.Documents;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace OoBDev.Apache.Tika.Detectors;

/// <summary>
/// Provides functionality to detect the content type of a stream using Apache Tika.
/// </summary>
public class TikaContentTypeDetector : IContentTypeDetector
{
    private readonly IApacheTikaClient _client;
    private readonly ILogger _logger;

    public TikaContentTypeDetector(
        IApacheTikaClient client,
        ILogger<TikaContentTypeDetector> logger
            )
    {
        _client = client;
        _logger = logger;
    }

    /// <summary>
    /// Asynchronously detects the content type of the provided stream using Apache Tika.
    /// </summary>
    /// <param name="source">The stream whose content type needs to be detected.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result
    /// contains the detected content type as a string, or <c>null</c> if the content type cannot be determined.
    /// </returns>
    public async Task<string?> DetectContentTypeAsync(Stream source) =>
        await _client.DetectStreamAsync(source);
}
