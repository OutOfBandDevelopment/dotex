using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.JwtAuthentication.OpenApi;

/// <summary>
/// Reads the authorization and token endpoints from an authority's <c>.well-known/openid-configuration</c> document.
/// </summary>
/// <param name="client">The client used for the request.</param>
/// <param name="logger">Logger</param>
public partial class OpenIdConfigurationDiscovery(
    HttpClient client,
    ILogger<OpenIdConfigurationDiscovery> logger
    )
{
    /// <summary>
    /// The longest a discovery request may take.
    /// </summary>
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    private readonly ConcurrentDictionary<string, Task<OpenIdEndpoints?>> _cache = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets the endpoints of the authority. A success is cached; a failure is logged and tried again on the next call.
    /// </summary>
    /// <param name="authority">The authority (issuer) URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The endpoints, or <see langword="null"/> when they cannot be read.</returns>
    public async Task<OpenIdEndpoints?> GetAsync(string authority, CancellationToken cancellationToken)
    {
        var task = _cache.GetOrAdd(authority, a => LoadAsync(a, cancellationToken));
        var result = await task.ConfigureAwait(false);
        if (result is null)
        {
            _cache.TryRemove(authority, out _);
        }
        return result;
    }

    private async Task<OpenIdEndpoints?> LoadAsync(string authority, CancellationToken cancellationToken)
    {
        var url = authority.TrimEnd('/') + "/.well-known/openid-configuration";
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(Timeout);
        try
        {
            var document = await client.GetFromJsonAsync<OpenIdConfigurationDocument>(url, timeout.Token).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(document?.AuthorizationEndpoint) || string.IsNullOrWhiteSpace(document.TokenEndpoint))
            {
                LogIncomplete(logger, url);
                return null;
            }
            return new OpenIdEndpoints(document.AuthorizationEndpoint, document.TokenEndpoint);
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or global::System.Text.Json.JsonException or NotSupportedException or UriFormatException or InvalidOperationException)
        {
            LogFailed(logger, url, ex);
            return null;
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "OpenID configuration at {Url} has no authorization or token endpoint")]
    private static partial void LogIncomplete(ILogger logger, string url);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "OpenID configuration at {Url} could not be read")]
    private static partial void LogFailed(ILogger logger, string url, Exception exception);

    private sealed class OpenIdConfigurationDocument
    {
        [JsonPropertyName("authorization_endpoint")]
        public string? AuthorizationEndpoint { get; set; }

        [JsonPropertyName("token_endpoint")]
        public string? TokenEndpoint { get; set; }
    }
}
