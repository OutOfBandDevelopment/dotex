using System.Collections.Generic;
using System.Net.Http;

namespace OoBDev.System.Net.Http;

/// <summary>
/// Coordinates multiple HTTP request preparation features to configure HttpClient and HttpRequestMessage instances.
/// Implements the composite pattern to apply multiple preparation steps in sequence.
/// </summary>
public class HttpPrepareRequest : IHttpPrepareRequest
{
    private readonly IEnumerable<IHttpPrepareRequestFeature> _features;

    /// <summary>
    /// Initializes a new instance of the HttpPrepareRequest class with the specified preparation features.
    /// </summary>
    /// <param name="features">The collection of features to apply during request preparation.</param>
    public HttpPrepareRequest(
        IEnumerable<IHttpPrepareRequestFeature> features
        )
    {
        _features = features;
    }

    /// <summary>
    /// Prepares an HTTP request by applying all configured features in sequence.
    /// Each feature can modify the HttpClient, HttpRequestMessage, or process the URL.
    /// </summary>
    /// <param name="client">The HttpClient to configure.</param>
    /// <param name="request">The HttpRequestMessage to configure.</param>
    /// <param name="url">The target URL for the request.</param>
    public void PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
    {
        foreach (var feature in _features)
        {
            feature.PrepareRequest(client, request, url);
        }
    }
}
