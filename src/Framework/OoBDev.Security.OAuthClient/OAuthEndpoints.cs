using System;
using System.Text.Json;

namespace OoBDev.Security.OAuthClient;

/// <summary>The endpoints read from the provider's discovery document.</summary>
/// <param name="Authorization">The authorization endpoint.</param>
/// <param name="Token">The token endpoint.</param>
/// <param name="DeviceAuthorization">The device authorization endpoint, when offered.</param>
/// <param name="Revocation">The revocation endpoint, when offered.</param>
public sealed record OAuthEndpoints(Uri Authorization, Uri Token, Uri? DeviceAuthorization, Uri? Revocation)
{
    /// <summary>Reads a discovery document.</summary>
    /// <param name="json">The JSON of <c>/.well-known/openid-configuration</c>.</param>
    /// <returns>The endpoints.</returns>
    /// <exception cref="OAuthException">The document has no authorization or token endpoint.</exception>
    public static OAuthEndpoints Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var authorization = Read(root, "authorization_endpoint");
        var token = Read(root, "token_endpoint");
        if (authorization is null || token is null)
            throw new OAuthException("invalid_discovery", "The discovery document has no authorization_endpoint or token_endpoint.");
        return new OAuthEndpoints(authorization, token, Read(root, "device_authorization_endpoint"), Read(root, "revocation_endpoint"));
    }

    private static Uri? Read(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String && Uri.TryCreate(value.GetString(), UriKind.Absolute, out var uri) ? uri : null;
}
