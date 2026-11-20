using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace OoBDev.Security.OAuthClient;

/// <summary>Public-client OAuth 2.0: PKCE on a loopback redirect, device code, refresh and revocation. Tokens are kept in the <see cref="ISecretStore"/>.</summary>
public sealed class PublicOAuthClient : IOAuthClient
{
    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http;
    private readonly OAuthClientOptions _options;
    private readonly ISecretStore _store;
    private readonly TimeProvider _time;
    private OAuthEndpoints? _endpoints;

    /// <summary>Creates the client.</summary>
    /// <param name="http">The HTTP client for the provider.</param>
    /// <param name="options">The settings.</param>
    /// <param name="store">Where tokens are kept.</param>
    /// <param name="time">The clock.</param>
    public PublicOAuthClient(HttpClient http, IOptions<OAuthClientOptions> options, ISecretStore store, TimeProvider? time = null)
    {
        _http = http;
        _options = options.Value;
        _store = store;
        _time = time ?? TimeProvider.System;
    }

    /// <inheritdoc />
    public async Task<OAuthTokens> SignInWithBrowserAsync(Action<Uri> openBrowser, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(openBrowser);
        var endpoints = await GetEndpointsAsync(cancellationToken).ConfigureAwait(false);
        var verifier = Pkce.CreateVerifier();
        var state = Pkce.CreateState();
        using var receiver = new LoopbackReceiver();
        var query = new Dictionary<string, string>
        {
            ["response_type"] = "code",
            ["client_id"] = _options.ClientId,
            ["redirect_uri"] = receiver.RedirectUri.AbsoluteUri,
            ["scope"] = string.Join(' ', _options.Scopes),
            ["state"] = state,
            ["code_challenge"] = Pkce.ChallengeFor(verifier),
            ["code_challenge_method"] = "S256",
        };
        var builder = new UriBuilder(endpoints.Authorization)
        {
            Query = string.Join('&', query.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}")),
        };
        openBrowser(builder.Uri);
        var code = await receiver.WaitForCodeAsync(state, _options.LoopbackTimeout, cancellationToken).ConfigureAwait(false);
        var tokens = await PostTokenAsync(endpoints.Token, new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = receiver.RedirectUri.AbsoluteUri,
            ["client_id"] = _options.ClientId,
            ["code_verifier"] = verifier,
        }, null, cancellationToken).ConfigureAwait(false);
        await SaveAsync(tokens, cancellationToken).ConfigureAwait(false);
        return tokens;
    }

    /// <inheritdoc />
    public async Task<OAuthTokens> SignInWithDeviceCodeAsync(Action<DeviceCodePrompt> showPrompt, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(showPrompt);
        var endpoints = await GetEndpointsAsync(cancellationToken).ConfigureAwait(false);
        if (endpoints.DeviceAuthorization is null)
            throw new OAuthException("unsupported_grant_type", "The provider does not offer the device authorization endpoint.");

        using var response = await _http.PostAsync(endpoints.DeviceAuthorization, Form(new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["scope"] = string.Join(' ', _options.Scopes),
        }), cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        if (!response.IsSuccessStatusCode)
            throw ToException(root);

        var deviceCode = root.GetProperty("device_code").GetString()!;
        var interval = TimeSpan.FromSeconds(root.TryGetProperty("interval", out var i) ? i.GetInt32() : 5);
        var expiresIn = TimeSpan.FromSeconds(root.TryGetProperty("expires_in", out var e) ? e.GetInt32() : 600);
        showPrompt(new DeviceCodePrompt(
            root.GetProperty("user_code").GetString()!,
            new Uri(root.GetProperty("verification_uri").GetString()!),
            root.TryGetProperty("verification_uri_complete", out var c) && c.GetString() is { } complete ? new Uri(complete) : null,
            expiresIn));

        var deadline = _time.GetUtcNow() + expiresIn;
        while (true)
        {
            await Task.Delay(interval, _time, cancellationToken).ConfigureAwait(false);
            if (_time.GetUtcNow() > deadline)
                throw new OAuthException("expired_token", "The device code expired before it was approved.");
            try
            {
                var tokens = await PostTokenAsync(endpoints.Token, new Dictionary<string, string>
                {
                    ["grant_type"] = "urn:ietf:params:oauth:grant-type:device_code",
                    ["device_code"] = deviceCode,
                    ["client_id"] = _options.ClientId,
                }, null, cancellationToken).ConfigureAwait(false);
                await SaveAsync(tokens, cancellationToken).ConfigureAwait(false);
                return tokens;
            }
            catch (OAuthException ex) when (ex.Error == "authorization_pending")
            {
            }
            catch (OAuthException ex) when (ex.Error == "slow_down")
            {
                interval += TimeSpan.FromSeconds(5);
            }
        }
    }

    /// <inheritdoc />
    public async Task<OAuthTokens> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(refreshToken);
        var endpoints = await GetEndpointsAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var tokens = await PostTokenAsync(endpoints.Token, new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = _options.ClientId,
            }, refreshToken, cancellationToken).ConfigureAwait(false);
            await SaveAsync(tokens, cancellationToken).ConfigureAwait(false);
            return tokens;
        }
        catch (OAuthException ex) when (ex.Error == "invalid_grant")
        {
            await _store.DeleteAsync(_options.SecretName, cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        var stored = await LoadAsync(cancellationToken).ConfigureAwait(false);
        await _store.DeleteAsync(_options.SecretName, cancellationToken).ConfigureAwait(false);
        if (stored?.RefreshToken is null)
            return;
        var endpoints = await GetEndpointsAsync(cancellationToken).ConfigureAwait(false);
        if (endpoints.Revocation is null)
            return;
        using var response = await _http.PostAsync(endpoints.Revocation, Form(new Dictionary<string, string>
        {
            ["token"] = stored.RefreshToken,
            ["token_type_hint"] = "refresh_token",
            ["client_id"] = _options.ClientId,
        }), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Reads the stored tokens.</summary>
    /// <param name="cancellationToken">Cancels the read.</param>
    /// <returns>The tokens, or null when nobody is signed in or the stored value is unreadable.</returns>
    public async Task<OAuthTokens?> LoadAsync(CancellationToken cancellationToken = default)
    {
        var text = await _store.ReadAsync(_options.SecretName, cancellationToken).ConfigureAwait(false);
        if (text is null)
            return null;
        try
        {
            return JsonSerializer.Deserialize<OAuthTokens>(text, _json);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private Task SaveAsync(OAuthTokens tokens, CancellationToken cancellationToken) =>
        _store.WriteAsync(_options.SecretName, JsonSerializer.Serialize(tokens, _json), cancellationToken);

    private async Task<OAuthEndpoints> GetEndpointsAsync(CancellationToken cancellationToken)
    {
        if (_endpoints is not null)
            return _endpoints;
        var issuer = _options.Issuer ?? throw new OAuthException("invalid_configuration", "No issuer is configured.");
        var address = new Uri(issuer.AbsoluteUri.TrimEnd('/') + "/.well-known/openid-configuration");
        var json = await _http.GetStringAsync(address, cancellationToken).ConfigureAwait(false);
        return _endpoints = OAuthEndpoints.Parse(json);
    }

    private async Task<OAuthTokens> PostTokenAsync(Uri endpoint, Dictionary<string, string> form, string? previousRefreshToken, CancellationToken cancellationToken)
    {
        using var response = await _http.PostAsync(endpoint, Form(form), cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(body);
        }
        catch (JsonException)
        {
            throw new OAuthException("invalid_response", $"The token endpoint answered {(int)response.StatusCode} with a body that is not JSON.");
        }

        using (document)
        {
            var root = document.RootElement;
            if (!response.IsSuccessStatusCode || root.TryGetProperty("error", out _))
                throw ToException(root);
            var expires = root.TryGetProperty("expires_in", out var e) ? e.GetInt32() : 300;
            return new OAuthTokens(
                root.GetProperty("access_token").GetString()!,
                root.TryGetProperty("refresh_token", out var r) ? r.GetString() : previousRefreshToken,
                _time.GetUtcNow().AddSeconds(expires),
                root.TryGetProperty("id_token", out var id) ? id.GetString() : null,
                root.TryGetProperty("scope", out var s) ? s.GetString() : null);
        }
    }

    private static OAuthException ToException(JsonElement root) =>
        new(root.TryGetProperty("error", out var error) ? error.GetString() ?? "invalid_response" : "invalid_response",
            root.TryGetProperty("error_description", out var d) ? d.GetString() : null);

    private static FormUrlEncodedContent Form(Dictionary<string, string> values) => new(values);
}
