using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.AuthGateway;

/// <summary>Client credentials and RFC 8693 token exchange against a token endpoint, with a per-key cache that ends shortly before expiry.</summary>
public sealed class OAuthTokenProvider(IHttpClientFactory httpClientFactory, IOptions<AuthGatewayOptions> options, TimeProvider timeProvider) : IOutboundTokenProvider
{
    public const string HttpClientName = "OoBDev.AuthGateway.Tokens";

    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new(StringComparer.Ordinal);

    public async Task<string> GetTokenAsync(string targetName, OutboundOptions outbound, GatewayCaller caller, string? subjectToken, CancellationToken cancellationToken = default)
    {
        var key = CacheKey(targetName, outbound, caller, subjectToken);
        var entry = _cache.GetOrAdd(key, _ => new CacheEntry());
        if (entry.Token is { } cached && IsFresh(cached))
        {
            return cached.Value;
        }

        await entry.Gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (entry.Token is { } again && IsFresh(again))
            {
                return again.Value;
            }

            var token = await RequestAsync(outbound, subjectToken, cancellationToken).ConfigureAwait(false);
            entry.Token = token;
            return token.Value;
        }
        finally
        {
            entry.Gate.Release();
        }
    }

    private bool IsFresh(AccessToken token) => token.ExpiresAt - options.Value.TokenSkew > timeProvider.GetUtcNow();

    private static string CacheKey(string targetName, OutboundOptions outbound, GatewayCaller caller, string? subjectToken)
    {
        var exchange = outbound.Grant == OutboundGrant.TokenExchange && subjectToken is not null
            ? Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(subjectToken)))
            : string.Empty;
        return $"{targetName}|{outbound.Grant}|{caller.Scheme}|{caller.Subject}|{exchange}";
    }

    private async Task<AccessToken> RequestAsync(OutboundOptions outbound, string? subjectToken, CancellationToken cancellationToken)
    {
        var form = new List<KeyValuePair<string, string>> { new("client_id", outbound.ClientId) };
        if (!string.IsNullOrEmpty(outbound.ClientSecret))
        {
            form.Add(new("client_secret", outbound.ClientSecret));
        }

        if (outbound.Grant == OutboundGrant.TokenExchange)
        {
            if (string.IsNullOrEmpty(subjectToken))
            {
                throw new TokenRequestException("A token exchange needs the caller's bearer token.");
            }

            form.Add(new("grant_type", "urn:ietf:params:oauth:grant-type:token-exchange"));
            form.Add(new("subject_token", subjectToken));
            form.Add(new("subject_token_type", "urn:ietf:params:oauth:token-type:access_token"));
            form.Add(new("requested_token_type", "urn:ietf:params:oauth:token-type:access_token"));
        }
        else
        {
            form.Add(new("grant_type", "client_credentials"));
        }

        if (!string.IsNullOrEmpty(outbound.Scope))
        {
            form.Add(new("scope", outbound.Scope));
        }

        if (!string.IsNullOrEmpty(outbound.Audience))
        {
            form.Add(new("audience", outbound.Audience));
        }

        try
        {
            using var client = httpClientFactory.CreateClient(HttpClientName);
            using var content = new FormUrlEncodedContent(form);
            using var response = await client.PostAsync(outbound.TokenEndpoint, content, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new TokenRequestException($"The token endpoint answered {(int)response.StatusCode}.");
            }

            var body = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken).ConfigureAwait(false);
            if (body is null || string.IsNullOrEmpty(body.AccessToken))
            {
                throw new TokenRequestException("The token endpoint returned no access token.");
            }

            return new AccessToken(body.AccessToken, timeProvider.GetUtcNow().AddSeconds(body.ExpiresIn > 0 ? body.ExpiresIn : 60));
        }
        catch (HttpRequestException ex)
        {
            throw new TokenRequestException("The token endpoint could not be reached.", ex);
        }
    }

    private sealed class CacheEntry
    {
        public SemaphoreSlim Gate { get; } = new(1, 1);

        public AccessToken? Token { get; set; }
    }

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string? AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
}
