using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace OoBDev.Security.OAuthClient;

/// <summary>Serves the stored access token and refreshes it once, however many callers ask at the same time.</summary>
public sealed class TokenCache : ITokenCache, IDisposable
{
    private readonly PublicOAuthClient _client;
    private readonly OAuthClientOptions _options;
    private readonly TimeProvider _time;
    private readonly SemaphoreSlim _gate = new(1, 1);

    /// <summary>Creates the cache.</summary>
    /// <param name="client">The client that stores and refreshes tokens.</param>
    /// <param name="options">The settings (<see cref="OAuthClientOptions.Skew"/>).</param>
    /// <param name="time">The clock.</param>
    public TokenCache(PublicOAuthClient client, IOptions<OAuthClientOptions> options, TimeProvider? time = null)
    {
        _client = client;
        _options = options.Value;
        _time = time ?? TimeProvider.System;
    }

    /// <inheritdoc />
    public async Task<string?> GetAccessTokenAsync(string? rejectedToken = null, CancellationToken cancellationToken = default)
    {
        var tokens = await _client.LoadAsync(cancellationToken).ConfigureAwait(false);
        if (tokens is not null && IsUsable(tokens, rejectedToken))
            return tokens.AccessToken;

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Another caller may have refreshed while this one waited.
            tokens = await _client.LoadAsync(cancellationToken).ConfigureAwait(false);
            if (tokens is null)
                return null;
            if (IsUsable(tokens, rejectedToken))
                return tokens.AccessToken;
            if (tokens.RefreshToken is null)
                return null;
            try
            {
                return (await _client.RefreshAsync(tokens.RefreshToken, cancellationToken).ConfigureAwait(false)).AccessToken;
            }
            catch (OAuthException ex) when (ex.Error == "invalid_grant")
            {
                return null;
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Releases the lock.</summary>
    public void Dispose() => _gate.Dispose();

    private bool IsUsable(OAuthTokens tokens, string? rejectedToken) =>
        tokens.ExpiresAt - _options.Skew > _time.GetUtcNow() && !string.Equals(tokens.AccessToken, rejectedToken, StringComparison.Ordinal);
}
