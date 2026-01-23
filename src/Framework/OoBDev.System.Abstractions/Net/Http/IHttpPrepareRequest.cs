using System.Net.Http;

namespace OoBDev.System.Net.Http;

/// <summary>
/// Provides functionality to prepare HTTP requests before they are sent.
/// </summary>
public interface IHttpPrepareRequest
{
    /// <summary>
    /// Prepares an HTTP request by modifying the client, request, or URL before the request is sent.
    /// </summary>
    /// <param name="client">The HTTP client that will send the request.</param>
    /// <param name="request">The HTTP request message to prepare.</param>
    /// <param name="url">The target URL for the request.</param>
    void PrepareRequest(HttpClient client, HttpRequestMessage request, string url);
}
