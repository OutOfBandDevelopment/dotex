using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Security.OAuthClient;

/// <summary>Adds the bearer token to outgoing requests and, after a 401, refreshes once and retries once.</summary>
public sealed class BearerTokenHandler : DelegatingHandler
{
    private readonly ITokenCache _cache;

    /// <summary>Creates the handler.</summary>
    /// <param name="cache">The token source.</param>
    public BearerTokenHandler(ITokenCache cache) => _cache = cache;

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _cache.GetAccessTokenAsync(null, cancellationToken).ConfigureAwait(false);
        if (token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode != HttpStatusCode.Unauthorized || token is null || request.Content is { } && !request.Content.Headers.ContentLength.HasValue)
            return response;

        var fresh = await _cache.GetAccessTokenAsync(token, cancellationToken).ConfigureAwait(false);
        if (fresh is null || fresh == token)
            return response;

        response.Dispose();
        using var retry = await CloneAsync(request, cancellationToken).ConfigureAwait(false);
        retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", fresh);
        return await base.SendAsync(retry, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<HttpRequestMessage> CloneAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri) { Version = request.Version };
        foreach (var header in request.Headers)
            if (header.Key != "Authorization")
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        if (request.Content is not null)
        {
            var bytes = await request.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            clone.Content = new ByteArrayContent(bytes);
            foreach (var header in request.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}
