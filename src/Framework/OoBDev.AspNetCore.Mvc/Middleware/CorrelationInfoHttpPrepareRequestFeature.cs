using OoBDev.System.Accessors;
using OoBDev.System.Net.Http;
using System;
using System.Net.Http;

namespace OoBDev.AspNetCore.Mvc.Middleware;

/// <summary>
/// HTTP request preparation feature that adds correlation and request ID headers to outgoing requests.
/// </summary>
public class CorrelationInfoHttpPrepareRequestFeature : IHttpPrepareRequestFeature
{
    private readonly IAccessor<CorrelationInfo> _accessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationInfoHttpPrepareRequestFeature"/> class.
    /// </summary>
    /// <param name="accessor">The accessor for correlation information.</param>
    public CorrelationInfoHttpPrepareRequestFeature(
        IAccessor<CorrelationInfo> accessor
        ) => _accessor = accessor;

    /// <summary>
    /// Prepares an HTTP request by adding correlation ID and request ID headers.
    /// </summary>
    /// <param name="client">The HTTP client.</param>
    /// <param name="request">The HTTP request message to prepare.</param>
    /// <param name="url">The request URL.</param>
    public void PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
    {
        var correlationId = _accessor.Value?.CorrelationId;
        var requestId = Guid.NewGuid().ToString();

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            request.Headers.TryAddWithoutValidation(DefinedHttpHeaders.CorrelationIdHeader, correlationId);
        }

        request.Headers.TryAddWithoutValidation(DefinedHttpHeaders.RequestIdHeader, requestId);
    }
}
