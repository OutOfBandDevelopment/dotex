using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace OoBDev.Keycloak.Identity;

/// <summary>
/// Reads users and realm roles through the Keycloak admin REST API. Read only: nothing here creates, changes or removes anything.
/// The service account token is cached until shortly before it expires.
/// </summary>
/// <param name="http">The client used for the token and admin calls.</param>
/// <param name="options">The server, realm and service account.</param>
/// <param name="time">The clock used to expire the cached token.</param>
public sealed class KeycloakDirectoryReader(HttpClient http, IOptions<KeycloakIdentityOptions> options, TimeProvider time) : IDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private string? _token;
    private DateTimeOffset _expires;

    /// <summary>Lists users, each with their direct realm roles.</summary>
    /// <param name="search">Text matched against user name, email and names; null lists all.</param>
    /// <param name="email">An exact email address to match; takes the place of <paramref name="search"/>.</param>
    /// <param name="max">The most users to return.</param>
    /// <param name="cancellationToken">Cancels the calls.</param>
    /// <returns>The users in the order Keycloak returned them.</returns>
    public async Task<IReadOnlyList<KeycloakUserSummary>> ListUsersAsync(string? search = null, string? email = null, int max = 50, CancellationToken cancellationToken = default)
    {
        var query = $"max={max}";
        if (!string.IsNullOrWhiteSpace(email))
        {
            query += $"&exact=true&email={Uri.EscapeDataString(email)}";
        }
        else if (!string.IsNullOrWhiteSpace(search))
        {
            query += $"&search={Uri.EscapeDataString(search)}";
        }

        var users = await GetAsync<List<UserDto>>($"users?{query}", cancellationToken).ConfigureAwait(false) ?? [];
        var result = new List<KeycloakUserSummary>(users.Count);
        foreach (var user in users)
        {
            var roles = await GetAsync<List<RoleDto>>($"users/{Uri.EscapeDataString(user.Id)}/role-mappings/realm", cancellationToken).ConfigureAwait(false) ?? [];
            result.Add(new KeycloakUserSummary(user.Id, user.Username, user.Email, user.FirstName, user.LastName, user.Enabled, [.. roles.Select(r => r.Name).Order(StringComparer.Ordinal)]));
        }

        return result;
    }

    /// <summary>Lists the realm role names.</summary>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The role names in ordinal order.</returns>
    public async Task<IReadOnlyList<string>> ListRolesAsync(CancellationToken cancellationToken = default) =>
        [.. (await GetAsync<List<RoleDto>>("roles", cancellationToken).ConfigureAwait(false) ?? []).Select(r => r.Name).Order(StringComparer.Ordinal)];

    private async Task<T?> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{settings.BaseUrl.TrimEnd('/')}/admin/realms/{Uri.EscapeDataString(settings.Realm)}/{path}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await TokenAsync(cancellationToken).ConfigureAwait(false));
        using var response = await http.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken).ConfigureAwait(false);
    }

    private async Task<string> TokenAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_token is not null && time.GetUtcNow() < _expires)
            {
                return _token;
            }

            var settings = options.Value;
            using var form = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new("grant_type", "client_credentials"),
                new("client_id", settings.ClientId),
                new("client_secret", settings.ClientSecret),
            });
            using var response = await http.PostAsync($"{settings.BaseUrl.TrimEnd('/')}/realms/{Uri.EscapeDataString(settings.Realm)}/protocol/openid-connect/token", form, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var token = await response.Content.ReadFromJsonAsync<TokenDto>(cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException("Keycloak returned an empty token response.");
            _token = token.AccessToken;
            _expires = time.GetUtcNow().AddSeconds(Math.Max(0, token.ExpiresIn - 30));
            return _token;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public void Dispose() => _gate.Dispose();

    private sealed record TokenDto(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);

    private sealed record RoleDto([property: JsonPropertyName("name")] string Name);

    private sealed record UserDto(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("username")] string Username,
        [property: JsonPropertyName("email")] string? Email,
        [property: JsonPropertyName("firstName")] string? FirstName,
        [property: JsonPropertyName("lastName")] string? LastName,
        [property: JsonPropertyName("enabled")] bool Enabled);
}
